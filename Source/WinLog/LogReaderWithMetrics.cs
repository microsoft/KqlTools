// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using WinLog.Helpers;
    using WinLog.LogHelpers;

    /// <summary>
    ///     This class transforms the local Windows event log API for events into
    ///     ly typed instances that can be serialized to Kusto
    /// </summary>
    public class LogReaderWithMetrics
    {
        public ulong RecordsSoFar; // for debugging

        public bool UseJsonFilter { get; set; }

        public string JsonFilter { get; private set; }

        public List<JsonParseFilter> FilterList { get; set; }

        public int EventCount { get; set; }

        public int SequenceNumber { get; set; }

        public int FilteredEventCount { get; set; }

        public int DataWithoutNamesEventCount { get; set; }

        public int NullEventDataCount { get; set; }

        public int NullUserDataCount { get; set; }

        public long LogFileId { get; set; }

        public string Collector { get; set; }

        public Dictionary<string, EventIdMetrics> EventIdMetricsList { get; set; } = new Dictionary<string, EventIdMetrics>();

        public List<string> EventIdWhiteList { get; set; } = new List<string>();

        public IEnumerable<LogRecordCdoc> ReadFile(string logPath, bool useJsonFilter, string jsonFilter, long logFileId, string collector,
            string eventIdWhiteList = null)
        {
            UseJsonFilter = useJsonFilter;
            JsonFilter = jsonFilter;
            EventCount = 0;
            FilteredEventCount = 0;
            SequenceNumber = 0;
            LogFileId = logFileId;
            Collector = collector;
            if (!string.IsNullOrEmpty(eventIdWhiteList))
            {
                EventIdWhiteList = eventIdWhiteList.Split('|').ToList();
            }

            if (!string.IsNullOrEmpty(jsonFilter))
            {
                // deserialize JSON directly from the database field
                FilterList = JsonConvert.DeserializeObject<List<JsonParseFilter>>(jsonFilter);
            }

            var log = EvtxEnumerable.ReadEvtxFile(logPath);
            return ReadAsJson(log);
        }

        public IEnumerable<LogRecordCdoc> ReadLog(string logName, EventBookmark bookmark)
        {
            var log = EvtxEnumerable.ReadWindowsLog(logName, bookmark);
            return ReadAsJson(log);
        }

        private IEnumerable<LogRecordCdoc> ReadAsJson(IEnumerable<EventRecord> log)
        {
            var cache = new ProviderStringCache();
            var systemPropertiesDictionary = new Dictionary<string, object>();
            foreach (var e in log)
            {
                RecordsSoFar++;
                EventCount++;
                FilteredEventCount++;

                var namedProperties = new Dictionary<string, string>();
                var dataWithoutNames = new List<string>();
                string eventXml = e.ToXml();

                var sanitizedXmlString = XmlVerification.VerifyAndRepairXml(eventXml);

                var xe = XElement.Parse(sanitizedXmlString);
                var eventData = xe.Element(ElementNames.EventData);
                var userData = xe.Element(ElementNames.UserData);

                if (userData != null)
                {
                    NullEventDataCount++;

                    namedProperties = CommonXmlFunctions.ParseUserData(userData).ToDictionary(x => x.Key, x => x.Value.ToString());
                }

                if (eventData != null)
                {
                    NullUserDataCount++;
                    var eventDataProperties = CommonXmlFunctions.ParseEventData(eventData);
                    namedProperties = eventDataProperties.ToDictionary(x => x.Key, x => x.Value.ToString());
                }

                systemPropertiesDictionary =
                    CommonXmlFunctions.ConvertSystemPropertiesToDictionary(xe);

                // Filter out known events which are not white listed, if the list is populated.
                if (EventIdWhiteList.Any() && !EventIdWhiteList.Contains(systemPropertiesDictionary["EventID"].ToString()))
                {
                    FilteredEventCount--;
                    continue;
                }

                string json;
                if (dataWithoutNames.Count > 0)
                {
                    if (namedProperties.Count > 0)
                    {
                        throw new Exception("Event that has both unnnamed and named data?");
                    }

                    json = JsonConvert.SerializeObject(dataWithoutNames, Formatting.Indented);

                    DataWithoutNamesEventCount++;
                }
                else
                {
                    // Determine if the event is one we're set to filter
                    if (UseJsonFilter)
                    {
                        var isMatched = false;
                        foreach (var filter in FilterList)
                        {
                            if (filter.EventId.Equals(e.Id.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                                namedProperties.ContainsKey(filter.DataName) &&
                                (namedProperties[filter.DataName] == filter.Contains))
                            {
                                isMatched = true;
                                break;
                            }
                        }

                        // throw away event...just continue
                        if (isMatched)
                        {
                            FilteredEventCount--;
                            continue;
                        }
                    }

                    json = JsonConvert.SerializeObject(namedProperties, Formatting.Indented);
                }

                SequenceNumber++;
                var logFileLineage = new LogFileLineage
                {
                    Collector = Collector,
                    LogFileId = LogFileId,
                    UploadMachine = Environment.MachineName,
                    Seq = SequenceNumber,
                    CollectorTimeStamp = DateTime.UtcNow,
                    CollectorUnixTimeStamp = DateTime.UtcNow.GetUnixTime()
                };
                var serializedLogFileLineage = JsonConvert.SerializeObject(logFileLineage, Formatting.Indented);

                // Increment the Event Metrics
                IncrementEventMetricItems(e, EventIdMetricsList, eventXml,
                    systemPropertiesDictionary["Channel"] == null ? string.Empty : systemPropertiesDictionary["Channel"].ToString());

                string level = string.Empty;
                string task = string.Empty;
                string opcode = string.Empty;
                string keywords = string.Empty;
                try
                {
                    cache.Lookup(e, out level, out task, out opcode, out keywords);
                }
                catch (Exception)
                {
                    // Do nothing, as the IEnumerable yield is unfavorably to an exception on this call.  
                    // Empty values are tolerable, and unavailable with this exception
                }

                yield return new LogRecordCdoc(e.Bookmark)
                {
                    EventRecordId = Convert.ToInt64(systemPropertiesDictionary["EventRecordID"]),
                    TimeCreated = Convert.ToDateTime(systemPropertiesDictionary["TimeCreated"]),
                    Computer = systemPropertiesDictionary["Computer"].ToString(),
                    ProcessId = e.ProcessId ?? 0,
                    ThreadId = e.ThreadId ?? 0,
                    Provider = systemPropertiesDictionary["Provider"].ToString(),
                    EventId = Convert.ToInt32(systemPropertiesDictionary["EventID"]),
                    Level = level,
                    Version = CommonXmlFunctions.GetSafeExpandoObjectValue(systemPropertiesDictionary, "Version"),
                    Channel = systemPropertiesDictionary["Channel"].ToString(),
                    Task = task,
                    Opcode = opcode,
                    EventData = json,
                    LogFileLineage = serializedLogFileLineage
                };
            }
        }

        private void IncrementEventMetricItems(EventRecord eventRecord, Dictionary<string, EventIdMetrics> eventMetricItems, string eventXml,
            string channel)
        {
            string hashFormat = "{0}~{1}~{2}";
            string hashKey = string.Format(hashFormat, eventRecord.Id, eventRecord.ProviderName, channel);

            if (eventMetricItems.ContainsKey(hashKey))
            {
                eventMetricItems[hashKey].EventCount++;
            }
            else
            {
                eventMetricItems.Add(hashKey, new EventIdMetrics
                {
                    EventId = eventRecord.Id,
                    EventChannel = channel,
                    EventProvider = eventRecord.ProviderName,
                    EventCount = 1,
                    EventXml = eventXml
                });
            }
        }
    }
}