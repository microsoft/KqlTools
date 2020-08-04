// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.EvtxEventXmlScrubber
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing.Reader;
    using System.Dynamic;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;

    public static class EvtxExtensions
    {
        private static readonly EventLog eventLog = new EventLog { Source = "LocalEventLogDataSource" };

        public static string CreateDataItem(this EventLogRecord eventRecord, string workspaceId)
        {
            string dataItemTemplate  = "<DataItem type=\"System.Event.LinkedData\" time=\"{EventTimeUTC}\" sourceHealthServiceId=\"{WorkspaceId}\"><EventOriginId>{7C384BE3-8EBD-4B86-A392-357AA34750C5}</EventOriginId><PublisherId>{{ProviderGuid}}</PublisherId><PublisherName>{Provider}</PublisherName><EventSourceName>{EventSource}</EventSourceName><Channel>{Channel}</Channel><LoggingComputer>{Computer}</LoggingComputer><EventNumber>{EventId}</EventNumber><EventCategory>{EventCategory}</EventCategory><EventLevel>{EventLevel}</EventLevel><UserName>N/A</UserName><RawDescription></RawDescription><LCID>1033</LCID><CollectDescription>True</CollectDescription><EventData><DataItem type=\"System.XmlData\" time=\"{EventTimeUTC}\" sourceHealthServiceId=\"{WorkspaceId}\">{EventData}</DataItem></EventData><EventDisplayNumber>{EventId}</EventDisplayNumber><EventDescription></EventDescription><ManagedEntityId>{D056ADDA-9675-7690-CC92-41AA6B90CC05}</ManagedEntityId><RuleId>{1F68E37D-EC73-9BD3-92D5-C236C995FA0A}</RuleId></DataItem>\r\n";

            DateTime timeCreated = (DateTime)eventRecord.TimeCreated;
            string tempWinEvent = dataItemTemplate;
            tempWinEvent = tempWinEvent.Replace("{WorkspaceId}", workspaceId);
            tempWinEvent = tempWinEvent.Replace("{ProviderGuid}", (eventRecord.ProviderId ?? Guid.Empty).ToString());
            tempWinEvent = tempWinEvent.Replace("{Provider}", eventRecord.ProviderName);
            tempWinEvent = tempWinEvent.Replace("{EventSource}", eventRecord.ProviderName);
            tempWinEvent = tempWinEvent.Replace("{Channel}", eventRecord.LogName ?? "Unknown");
            tempWinEvent = tempWinEvent.Replace("{Computer}", eventRecord.MachineName);
            tempWinEvent = tempWinEvent.Replace("{EventId}", eventRecord.Id.ToString());
            tempWinEvent = tempWinEvent.Replace("{EventCategory}", (eventRecord.Task ?? 0).ToString());
            tempWinEvent = tempWinEvent.Replace("{EventLevel}", (eventRecord.Level ?? 0).ToString());
            tempWinEvent = tempWinEvent.Replace("{EventTimeUTC}", $"{ timeCreated.ToUniversalTime():yyyy-MM-ddTHH:mm:ss.ffffffZ}");
            tempWinEvent = tempWinEvent.Replace("{EventData}", RetrieveExtendedData(eventRecord.ToXml()));
            return tempWinEvent;
        }

        public static IDictionary<string, object> Deserialize(string eventXml)
        {
            var beforeCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                var sanitizedXmlString = XmlScrubber.VerifyAndRepairXml(eventXml);
                var xe = XElement.Parse(sanitizedXmlString);

                var systemData = xe.Element(ElementNames.System);
                Dictionary<string, object> instance = XmlEventParseHelpers.ConvertSystemPropertiesToDictionary(xe);

                var eventData = xe.Element(ElementNames.EventData);
                var userData = xe.Element(ElementNames.UserData);

                // Convert the EventData to named properties
                if (eventData != null)
                {
                    instance["EventData"] = XmlEventParseHelpers.ParseEventData(eventData);
                }

                // An event will never have EventData and UserData.
                // If there is UserData, then it should replace EventData.
                if (userData != null)
                {
                    instance["EventData"] = XmlEventParseHelpers.ParseUserData(userData);
                }

                return instance;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = beforeCulture;
            }
        }

        public static IDictionary<string, object> Deserialize(this EventLogRecord e, bool includeBookmark = false)
        {
            var beforeCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                var sanitizedXmlString = XmlScrubber.VerifyAndRepairXml(e.ToXml());
                var xe = XElement.Parse(sanitizedXmlString);

                var systemData = xe.Element(ElementNames.System);
                Dictionary<string, object> instance = XmlEventParseHelpers.ConvertSystemPropertiesToDictionary(xe);

                var eventData = xe.Element(ElementNames.EventData);
                var userData = xe.Element(ElementNames.UserData);

                // Convert the EventData to named properties
                if (eventData != null)
                {
                    instance["EventData"] = XmlEventParseHelpers.ParseEventData(eventData);
                }

                // Convert the EventData to named properties
                if (userData != null)
                {
                    instance["UserData"] = XmlEventParseHelpers.ParseUserData(userData);
                }

                if (includeBookmark)
                {
                    instance.Add("BookmarkChannel", GetBookmarkChannel(e.Bookmark));
                }

                return instance;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = beforeCulture;
            }
        }

        public static IDictionary<string, object> OptimizedDeserialize(this EventLogRecord e)
        {
            var beforeCulture = Thread.CurrentThread.CurrentCulture;

            try
            {
                string eventXml = e.ToXml();

                var xe = XElement.Parse(eventXml);
                var eventData = xe.Element(ElementNames.EventData);

                var instance = XmlEventParseHelpers.ConvertSystemPropertiesToDictionary(xe);
                instance.Add("BookmarkChannel", GetBookmarkChannel(e.Bookmark));

                if (eventData != null)
                {
                    var eventDataProperties = XmlEventParseHelpers.ParseEventData(eventData);
                    var namedProperties = eventDataProperties.ToDictionary(x => x.Key, x => x.Value);
                    instance["EventData"] = namedProperties;
                }
                else
                {
                    instance.Add("EventData", new Dictionary<string, object>());
                }

                return instance;
            }
            catch
            {
                return Deserialize(e); // Scrub only if any error occurs while deserializing.
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = beforeCulture;
            }
        }

        /// <summary>
        ///     Parse a single event into dynamic object type, from the xml of the Windows Event
        /// </summary>
        /// <param name="eventXml">the xml string of an EventRecord object</param>
        /// <returns>a dynamic representing the windows event</returns>
        public static string RetrieveExtendedData(string eventXml)
        {
            try
            {
                eventXml = XmlScrubber.VerifyAndRepairXml(eventXml);
                var xe = XElement.Parse(eventXml);

                var eventData = xe.Element(ElementNames.EventData);
                // Convert the EventData string
                if (eventData != null)
                {
                    return eventData.ToString();
                }

                var userData = xe.Element(ElementNames.UserData);
                // Return the UserData string
                if (userData != null)
                {
                    return userData.ToString();
                }

                // If the event has neither EventData or UserData, return null...  
                return null;
            }
            catch (Exception ex)
            {
                // Log Exception and return null
                EventInstance eventInstance = new EventInstance(101, 0, EventLogEntryType.Error);
                eventLog.WriteEvent(eventInstance, eventXml, ex.ToString());
                return null;
            }
        }

        private static string GetBookmarkChannel(EventBookmark eventBookmark)
        {
            var prop = typeof(EventBookmark).GetProperty("BookmarkText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var text = (string)prop.GetValue(eventBookmark);

            var xBookmark = XElement.Parse(text);
            string channel = string.Empty;
            foreach (var bookmarkElement in xBookmark.Descendants("Bookmark"))
            {
                channel = bookmarkElement.Attribute("Channel").Value;
            }

            return channel;
        }

        private static ExpandoObject GetNestedAttributes(XElement extendedElement)
        {
            ExpandoObject namedProperties = new ExpandoObject();

            foreach (var data in extendedElement.Elements())
            {
                foreach (XElement element in data.Elements())
                {
                    var name = element.Name.LocalName;
                    var value = element.Value.Replace("\"", "'");

                    // If the same item is being added[rare occurrance], continue
                    var dict = (IDictionary<string, object>)namedProperties;
                    if (dict.ContainsKey(name))
                    {
                        if (dict[name].Equals(value))
                        {
                            continue;
                        }

                        dict[name] = dict[name] + $" - {value}";
                        continue;
                    }

                    dict.Add(name, value);
                }
            }

            return namedProperties;
        }
    }
}
