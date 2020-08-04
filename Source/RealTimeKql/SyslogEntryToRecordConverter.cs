// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace RealTimeKql
{
    using Microsoft.Syslog.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Converts syslog entries to Kusto/server records. </summary>
    public class SyslogEntryToRecordConverter : Observable<IDictionary<string, object>>, IObserver<ServerSyslogEntry>
    {
        public void OnNext(ServerSyslogEntry serverEntry)
        {
            var rec = Convert(serverEntry);
            Broadcast(rec.AsDictionary()); 
        }

        public void OnCompleted()
        {
            BroadcastOnCompleted();
        }

        public void OnError(Exception error)
        {
            BroadcastError(error);
        }

        public static KustoSyslogRecord Convert(ServerSyslogEntry serverEntry)
        {
            // entry.StructuredData is RFC-5424 structured data; 
            // ExtractedData is key-values extracted from syslog body for other formats - we add SturcturedData values there as well
            var allParams = new List<NameValuePair>();
            allParams.AddRange(serverEntry.Entry.ExtractedData);            
            
            // This is RFC-5424 specific data, structured in its own way - we take all parameters (kv pairs) from there
            var structData = serverEntry.Entry.StructuredData;
            if (structData != null)
            {
                var structDataParams = structData.SelectMany(de => de.Value).ToList();
                allParams.AddRange(structDataParams); 
            }

            // now convert to dictionary; first group param values by param name
            var grouped = allParams.GroupBy(p => p.Name, p => p.Value).ToDictionary(g => g.Key, g => g.ToArray());
            // copy to final dictionary - keep multiple values as an array; 
            // for single value put it as a single object (not object[1]); - except for IP addresses - these are always arrays, even if there's only one
            var allDataDict = new Dictionary<string, object>(); 
            foreach(var kv in grouped)
            {
                if (kv.Value.Length > 1 || kv.Key == "IPv4" || kv.Key == "IPv6")
                {
                    allDataDict[kv.Key] = kv.Value; //array
                }
                else
                    allDataDict[kv.Key] = kv.Value[0]; //single value
            }

            var logFileLineage = new Dictionary<string, object>
            {
                ["PayloadType"] = serverEntry.Entry.PayloadType.ToString(),
                ["RelayServer"] = Environment.MachineName,
                ["ReceivedTimestamp"] = serverEntry.UdpPacket?.ReceivedOn,

            };

            // add parser errors if any
            var parserErrors = serverEntry.ParseErrorMessages;
            if (parserErrors != null && parserErrors.Count > 0)
            {
                logFileLineage["SyslogParserErrors"] = string.Join("; ", parserErrors);
            }

            var hdr = serverEntry.Entry.Header;
            var rec = new KustoSyslogRecord()
            {
                DeviceTimestamp = hdr.Timestamp,
                Facility = serverEntry.Entry.Facility.ToString(),
                Severity = serverEntry.Entry.Severity.ToString(),
                HostName = hdr.HostName,
                AppName = hdr.AppName,
                ProcId = hdr.ProcId,
                MsgId = hdr.MsgId,
                ExtractedData = allDataDict,
                Payload = serverEntry.Payload,
                LogFileLineage = logFileLineage,
                SourceIpAddress = serverEntry.UdpPacket?.SourceIpAddress?.ToString(),
            };
            return rec; 
        }
    }
}
