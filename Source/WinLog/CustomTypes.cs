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
    using Newtonsoft.Json;
    using WinLog.Helpers;

    public class EventIdMetrics
    {
        public int EventId { get; set; }

        public string EventProvider { get; set; }

        public int EventCount { get; set; }

        public string EventXml { get; set; }

        public string EventChannel { get; set; }
    }

    public class EventLogUploadResult
    {
        public int EventCount { get; set; }

        public int FilteredEventCount { get; set; }

        public double TimeToRead { get; set; }

        public double TimeToUpload { get; set; }

        public bool UploadSuccessful { get; set; }

        public string ErrorMessage { get; set; }

        public Dictionary<string, EventIdMetrics> EventIdMetricsList { get; set; }

        public int NullUserDataCount { get; set; }

        public int NullEventDataCount { get; set; }
    }

    public class LogRecord
    {
        public string Provider;
        public int EventId;
        public string Version;
        public string Level;
        public string Task;
        public string Opcode;
        public string Keywords;
        public DateTime TimeCreated;
        public long EventRecordId;
        public Guid Correlation; //missing in LogRecordCdoc
        public int ProcessId;
        public int ThreadId;
        public string Channel;
        public string Computer;
        public string Security;
        public dynamic EventData;
        public dynamic LogFileLineage;

        public EventBookmark Bookmark { get; private set; }

        public LogRecord(EventBookmark bookmark)
        {
            Bookmark = bookmark;
        }

        public LogRecord()
        {
        }

        public LogRecord(dynamic record, EventBookmark bookmark)
        {
            Bookmark = bookmark;

            SetCommonAttributes(record);
        }

        internal LogRecord(dynamic record)
        {
            SetCommonAttributes(record);
        }

        private void SetCommonAttributes(dynamic record)
        {
            IDictionary<string, object> dictionaryRecord = record;

            Provider = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Provider");
            EventId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "EventId"));
            TimeCreated = Convert.ToDateTime(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "TimeCreated"));
            Computer = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Computer");
            EventRecordId = Convert.ToInt64(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "EventRecordId"));

            if (dictionaryRecord.ContainsKey("EventData"))
            {
                EventData = JsonConvert.SerializeObject(dictionaryRecord["EventData"], Formatting.Indented);
            }

            // Newly added properties
            Version = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Version");
            Level = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Level");
            Task = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Task");
            Opcode = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Opcode");
            Security = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Security");
            Channel = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Channel");

            Keywords = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Keywords");

            Guid resultCorrelation;
            if (Guid.TryParse(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Correlation"), out resultCorrelation))
            {
                Correlation = resultCorrelation;
            }

            // Variant System properties (not on all Windows Events)
            string processId = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ProcessID");
            if (!string.IsNullOrEmpty(processId))
            {
                ProcessId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ProcessID"));
            }

            string threadId = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ThreadID");
            if (!string.IsNullOrEmpty(threadId))
            {
                ThreadId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ThreadID"));
            }
        }
    }

    public class LogRecordCdoc
    {
        public string Provider;
        public int EventId;
        public string Version;
        public string Level;
        public string Task;
        public string Opcode;
        public DateTime TimeCreated;
        public long EventRecordId;
        public int ProcessId;
        public int ThreadId;
        public string Channel;
        public string Computer;
        public string Security;
        public dynamic EventData;
        public dynamic LogFileLineage;

        public EventBookmark Bookmark { get; private set; }

        public LogRecordCdoc(EventBookmark bookmark)
        {
            Bookmark = bookmark;
        }

        public LogRecordCdoc()
        {
        }

        public LogRecordCdoc(dynamic record, EventBookmark bookmark)
        {
            Bookmark = bookmark;

            SetCommonAttributes(record);
        }

        internal LogRecordCdoc(dynamic record)
        {
            SetCommonAttributes(record);
        }

        private void SetCommonAttributes(dynamic record)
        {
            IDictionary<string, object> dictionaryRecord = record;

            Provider = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Provider");
            EventId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "EventId"));
            TimeCreated = Convert.ToDateTime(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "TimeCreated"));
            Computer = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Computer");
            EventRecordId = Convert.ToInt64(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "EventRecordId"));

            EventData = JsonConvert.SerializeObject(record.EventData, Formatting.Indented);

            // Newly added properties
            Version = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Version");
            Level = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Level");
            Task = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Task");
            Opcode = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Opcode");
            Security = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Security");
            Channel = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "Channel");

            // Variant System properties (not on all Windows Events)
            string processId = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ProcessID");
            if (!string.IsNullOrEmpty(processId))
            {
                ProcessId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ProcessID"));
            }

            string threadId = CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ThreadID");
            if (!string.IsNullOrEmpty(threadId))
            {
                ThreadId = Convert.ToInt32(CommonXmlFunctions.GetSafeExpandoObjectValue(dictionaryRecord, "ThreadID"));
            }
        }
    }

    public class LogRecordEx
    {
        public string Provider;
        public int EventId;
        public string Version;
        public string Level;
        public string Task;
        public string Opcode;
        public DateTime TimeCreated;
        public long EventRecordId;
        public int ProcessId;
        public int ThreadId;
        public string Channel;
        public string Computer;
        public string Security;
        public dynamic EventData;
        public dynamic EventContext;
        public dynamic LogFileLineage;

        public LogRecordEx(EventBookmark bookmark)
        {
            Bookmark = bookmark;
        }

        public LogRecordEx()
        {
        }

        public EventBookmark Bookmark { get; private set; }
    }

    public class JsonParseFilter
    {
        public JsonParseFilter()
        {
        }

        public JsonParseFilter(string eventId, string dataName, string contains)
        {
            EventId = eventId;
            DataName = dataName;
            Contains = contains;
        }

        public string EventId { get; set; }

        public string DataName { get; set; }

        public string Contains { get; set; }
    }

    public class LogFileLineage
    {
        public string Collector { get; set; }

        public string UploadMachine { get; set; }

        public long LogFileId { get; set; }

        public long Seq { get; set; }

        public DateTime CollectorTimeStamp { get; set; }

        public string CollectorUnixTimeStamp { get; set; }
    }
}