// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;

    [DebuggerDisplay("{Header}")]
    public class SyslogEntry
    {
        public PayloadType PayloadType;
        public Facility Facility;
        public Severity Severity;
        public SyslogHeader Header;
        public string Message;

        /// <summary>The StructuredData element for entries following RFC-5424 spec. It is a list of named elements, 
        /// each having a list of name-value pairs. </summary>
        /// <remarks>Keys inside element can be reapeated (see RFC 5424, example with ip parameter), so element value is a list of pairs, not dictionary.</remarks>
        public IDictionary<string, IList<NameValuePair>> StructuredData = new Dictionary<string, IList<NameValuePair>>();

        // same here, names can be repeated; 
        /// <summary>Data extracted from text message sections using various extraction methods, mostly pattern-matching. Keys/names can be repeated, 
        /// so the data is represented as list of key-value pairs, not dictionary. </summary>
        public IList<NameValuePair> ExtractedData = new List<NameValuePair>();

        /// <summary>All data, parsed and extracted, represented as a Dictionary. Values from the <see cref="StructuredData"/> and <see cref="ExtractedData"/>
        /// are combined in this single dictionary. </summary>
        /// <remarks>Each value in a dictionary is either a single value, or an array of strings, if there is more than one value.
        /// The exception is IPv4 and IPv6 entries, which are always arrays, even if there is just one value. This is done for conveniences 
        /// of querying the data in databases like Kusto. </remarks>
        public IDictionary<string, object> AllData = new Dictionary<string, object>();

        public SyslogEntry() {
            Header = new SyslogHeader(); 
        }

        public SyslogEntry(Facility facility, Severity severity, DateTime? timestamp = null, string hostName = null, 
                           string appName = null, string procId = null, string msgId = null, string message = null) 
        {
            PayloadType = PayloadType.Rfc5424;
            Facility = facility;
            Severity = severity;
            timestamp = timestamp ?? DateTime.UtcNow;
            Header = new SyslogHeader()
            {
                Timestamp = timestamp,  HostName = hostName,
                AppName = appName, ProcId = procId, MsgId = msgId //Version should always be 1
            };
            Message = message;
        }
        public override string ToString() => Header?.ToString(); 
    }

    public class SyslogHeader
    {
        public DateTime? Timestamp; 
        public string HostName;
        public string AppName;
        public string ProcId;
        public string MsgId;
        public override string ToString() => $"{Timestamp} host:{HostName} app: {AppName}";
    }

    [DebuggerDisplay("{Name}={Value}")]
    public class NameValuePair
    {
        public string Name;
        public string Value; 
    }

    public class UdpPacket
    {
        public DateTime ReceivedOn;
        public IPAddress SourceIpAddress;
        public byte[] Data;
    }

    [DebuggerDisplay("{Payload}")]
    public class ServerSyslogEntry
    {
        public UdpPacket UdpPacket;
        public string Payload; //entire syslog string 
        public SyslogEntry Entry;
        public IList<string> ParseErrorMessages;
        public bool Ignore; // filtered out

        public override string ToString() => $"{Payload}";
    }

}
