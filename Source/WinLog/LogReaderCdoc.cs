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
    using System.Dynamic;
    using System.Xml.Linq;
    using WinLog.Helpers;
    using WinLog.LogHelpers;

    /// <summary>
    ///     Creates enumerables, single event parsing, reading AP logs
    /// </summary>
    public class LogReaderCdoc
    {
        /// <summary>
        ///     Read an EVTX file
        /// </summary>
        /// <param name="fileName">the EVTX file name</param>
        /// <returns></returns>
        public static IEnumerable<LogRecord> ReadEvtxFile(string fileName)
        {
            long eventCount = 0; // for debugging

            using (var reader = new EventLogReader(fileName, PathType.FilePath))
            {
                for (;;)
                {
                    var record = reader.ReadEvent();
                    if (record == null)
                    {
                        yield break;
                    }

                    eventCount++;
                    dynamic evt = ParseEvent(record);
                    yield return new LogRecord(evt);
                }
            }
        }

        public static IEnumerable<LogRecord> ReadWindowsLog(string logName)
        {
            var log = EvtxEnumerable.ReadWindowsLog(logName, null);
            foreach (var e in log)
            {
                var evt = LogReader.ParseEvent(e);
                yield return new LogRecord(evt);
            }
        }

        public static IEnumerable<LogRecord> ReadWindowsLog(string logName, EventBookmark bookmark)
        {
            var log = EvtxEnumerable.ReadWindowsLog(logName, bookmark);
            foreach (var e in log)
            {
                var evt = LogReader.ParseEvent(e);
                yield return new LogRecord(evt, e.Bookmark);
            }
        }

        /// <summary>
        ///     Parse a single event into dynamic object type, from the XML of the windows event
        /// </summary>
        /// <param name="eventXml">the xml string of an EventRecord object</param>
        /// <returns></returns>
        public static dynamic ParseEvent(EventRecord eventRecord)
        {
            try
            {
                dynamic evt = ParseEvent(eventRecord.ToXml());

                evt.Bookmark = eventRecord.Bookmark;

                return evt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        ///     Parse a single event into dynamic object type, from the XML of the windows event
        /// </summary>
        /// <param name="eventXml">the xml string of an EventRecord object</param>
        /// <returns></returns>
        public static dynamic ParseEvent(string eventXml)
        {
            try
            {
                var sanitizedXmlString = XmlVerification.VerifyAndRepairXml(eventXml);
                var xe = XElement.Parse(sanitizedXmlString);

                var eventDataProperties = new ExpandoObject();

                var systemData = xe.Element(ElementNames.System);
                dynamic evt = CommonXmlFunctions.ParseSystemData(systemData);

                var eventData = xe.Element(ElementNames.EventData);
                var userData = xe.Element(ElementNames.UserData);

                // Convert the EventData to named properties
                if (eventData != null)
                {
                    eventDataProperties = CommonXmlFunctions.ParseEventData(eventData);
                    evt.EventData = eventDataProperties;
                }

                // Convert the EventData to named properties
                if (userData != null)
                {
                    evt.EventData = CommonXmlFunctions.ParseUserData(userData);
                }

                return evt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}