// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace RealTimeKql
{
    using System;
    using System.Collections.Generic;

    public class KustoSyslogRecord
    {
        public DateTime? DeviceTimestamp { get; set; }
        public string Facility { get; set; }
        public string Severity { get; set; }
        public string HostName { get; set; }
        public string SourceIpAddress { get; set; }
        public IDictionary<string, object> ExtractedData { get; set; }
        public string AppName { get; set; }
        public string ProcId { get; set; }
        public string MsgId { get; set; }
        public string Payload { get; set; } //entire syslog string 
        public IDictionary<string, object> LogFileLineage { get; set; } //inside: PayloadType, RelayServer, ReceivedOn;
    }
}
