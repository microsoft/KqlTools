// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using Newtonsoft.Json;

    public class WindowsLogEvent
    {
        public string Computer;
        public dynamic EventData;
        public int EventId;
        public string Provider;
        public DateTime TimeCreated;

        //public string Channel;
        //public dynamic EventData;
        //public long EventRecordId;
        //public string Level;
        //public dynamic LogFileLineage;
        //public string Opcode;
        //public int ProcessId;
        //public string Security;
        //public string Task;
        //public int ThreadId;
        //public string Version;

        public WindowsLogEvent(dynamic record)
        {
            Provider = record.Provider;
            EventId = record.EventId;
            TimeCreated = record.TimeCreated;
            Computer = record.Computer;
            EventData = JsonConvert.SerializeObject(record.EventData, Formatting.Indented);
        }
    }
}