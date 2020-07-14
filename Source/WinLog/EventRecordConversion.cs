// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Xml.Linq;
    using Newtonsoft.Json;
    using WinLog.Helpers;

    /// <summary>
    ///     The Event Record Conversion class
    /// </summary>
    public class EventRecordConversion : IDisposable
    {
        private readonly ProviderStringCache providerStringCache = new ProviderStringCache();

        public void Dispose()
        {
            // No actions to dispose of, yet...  Implemented to support the dispose pattern
        }

        /// <summary>
        ///     Converts a Windows EventRecord into a JsonLogRecord, used to insert to Kusto
        /// </summary>
        /// <param name="eventRecord">the EventRecord object</param>
        /// <returns></returns>
        public LogRecordCdoc ToLogRecordCdoc(EventRecord eventRecord)
        {
            if (eventRecord == null)
            {
                throw new ArgumentNullException(nameof(eventRecord));
            }

            string level;
            string task;
            string opCode;
            string keywords;
            providerStringCache.Lookup(eventRecord, out level, out task, out opCode, out keywords);

            return ToLogRecordCdoc(
                eventRecord.ToXml(),
                eventRecord.Bookmark,
                level,
                task,
                opCode,
                eventRecord.ProcessId ?? 0,
                eventRecord.ThreadId ?? 0);
        }

        /// <summary>
        ///     Converts a Windows EventRecord into a JsonLogRecordEx, containing an Extended field for use, used to insert to
        ///     Kusto
        /// </summary>
        /// <param name="eventXml"></param>
        /// <param name="eventBookmark"></param>
        /// <returns></returns>
        public LogRecordEx ToLogRecordEx(string eventXml,
            EventBookmark eventBookmark = null)
        {
            LogRecordCdoc logRecordCdoc = ToLogRecordCdoc(eventXml, eventBookmark);

            return new LogRecordEx
            {
                EventRecordId = logRecordCdoc.EventRecordId,
                TimeCreated = logRecordCdoc.TimeCreated,
                Computer = logRecordCdoc.Computer,
                ProcessId = logRecordCdoc.ProcessId,
                ThreadId = logRecordCdoc.ThreadId,
                Provider = logRecordCdoc.Provider,
                EventId = logRecordCdoc.EventId,
                Level = logRecordCdoc.Level,
                Version = logRecordCdoc.Version,
                Channel = logRecordCdoc.Channel,
                Task = logRecordCdoc.Task,
                Opcode = logRecordCdoc.Opcode,
                EventData = logRecordCdoc.EventData,
                LogFileLineage = logRecordCdoc.LogFileLineage
            };
        }

        /// <summary>
        ///     Creates a JsonLogRecord object
        /// </summary>
        /// <param name="eventXml"></param>
        /// <param name="eventBookmark"></param>
        /// <param name="level"></param>
        /// <param name="task"></param>
        /// <param name="opCode"></param>
        /// <param name="processId"></param>
        /// <param name="threadId"></param>
        /// <returns></returns>
        public LogRecordCdoc ToLogRecordCdoc(
            string eventXml,
            EventBookmark eventBookmark,
            string level = "",
            string task = "",
            string opCode = "",
            int processId = 0,
            int threadId = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(eventXml))
                {
                    throw new ArgumentNullException(nameof(eventXml));
                }

                var sanitizedXmlString = XmlVerification.VerifyAndRepairXml(eventXml);

                var xe = XElement.Parse(sanitizedXmlString);
                var eventData = xe.Element(ElementNames.EventData);
                var userData = xe.Element(ElementNames.UserData);

                var header = xe.Element(ElementNames.System);
                var recordId = long.Parse(header.Element(ElementNames.EventRecordId).Value);

                var systemPropertiesDictionary = CommonXmlFunctions.ConvertSystemPropertiesToDictionary(xe);

                var namedProperties = new Dictionary<string, string>();
                var dataWithoutNames = new List<string>();

                // Convert the EventData to named properties
                if (userData != null)
                {
                    namedProperties = CommonXmlFunctions.ParseUserData(userData).ToDictionary(x => x.Key, x => x.Value.ToString());
                }

                if (eventData != null)
                {
                    var eventDataProperties = CommonXmlFunctions.ParseEventData(eventData);
                    namedProperties = eventDataProperties.ToDictionary(x => x.Key, x => x.Value.ToString());
                }

                string json;
                if (dataWithoutNames.Count > 0)
                {
                    if (namedProperties.Count > 0)
                    {
                        throw new Exception("Event that has both unnnamed and named data?");
                    }

                    json = JsonConvert.SerializeObject(dataWithoutNames, Formatting.Indented);
                }
                else
                {
                    json = JsonConvert.SerializeObject(namedProperties, Formatting.Indented);
                }

                var collectorTimestamp = DateTime.UtcNow;
                var logFileLineage = new LogFileLineage
                {
                    LogFileId = 0,
                    UploadMachine = Environment.MachineName,
                    Seq = 1,
                    CollectorTimeStamp = collectorTimestamp,
                    CollectorUnixTimeStamp = collectorTimestamp.GetUnixTime()
                };

                string[] executionProcessThread;
                if (systemPropertiesDictionary.ContainsKey("Execution"))
                {
                    executionProcessThread = systemPropertiesDictionary["Execution"].ToString()
                        .Split(new[]
                        {
                            ':'
                        }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    executionProcessThread = new string[]
                    {
                        "0",
                        "0"
                    };
                }

                var serializedLogFileLineage = JsonConvert.SerializeObject(logFileLineage);

                return new LogRecordCdoc()
                {
                    EventRecordId = Convert.ToInt64(systemPropertiesDictionary["EventRecordID"]),
                    TimeCreated = Convert.ToDateTime(systemPropertiesDictionary["TimeCreated"]),
                    Computer = systemPropertiesDictionary["Computer"].ToString(),
                    ProcessId = processId.Equals(0) ? Convert.ToInt32(executionProcessThread[0]) : processId,
                    ThreadId = processId.Equals(0) ? Convert.ToInt32(executionProcessThread[1]) : threadId,
                    Provider = systemPropertiesDictionary["Provider"].ToString(),
                    EventId = Convert.ToInt32(systemPropertiesDictionary["EventID"]),
                    Level = !level.Equals(string.Empty) ? systemPropertiesDictionary["Level"].ToString() : level,
                    Version = CommonXmlFunctions.GetSafeExpandoObjectValue(systemPropertiesDictionary, "Version"),
                    Channel = systemPropertiesDictionary["Channel"].ToString(),
                    Security = CommonXmlFunctions.GetSafeExpandoObjectValue(systemPropertiesDictionary, "Security"),
                    Task = !task.Equals(string.Empty) ? systemPropertiesDictionary["Task"].ToString() : task,
                    Opcode = opCode,
                    EventData = json,
                    LogFileLineage = serializedLogFileLineage
                };
            }
            catch (Exception ex)
            {
                Trace.TraceError($"WinLog.EventRecordConversion.ToJsonLogRecord() threw an exception: {ex}");
                return null;
            }
        }
    }
}