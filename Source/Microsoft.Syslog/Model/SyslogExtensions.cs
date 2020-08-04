// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class SyslogExtensions
    {
        public static void AddRange(this IList<NameValuePair> bucket, IList<NameValuePair> prms)
        {
            foreach (var prm in prms)
                bucket.Add(prm);
        }

        public static void Add(this IList<NameValuePair> bucket, string name, string value)
        {
            bucket.Add(new NameValuePair() { Name = name, Value = value });
        }

        public static void BuildAllDataDictionary(this SyslogEntry entry)
        {
            // entry.StructuredData is RFC-5424 structured data; 
            // ExtractedData is key-values extracted from syslog body for other formats
            var allParams = new List<NameValuePair>();
            allParams.AddRange(entry.ExtractedData);

            // This is RFC-5424 specific data, structured in its own way - we take all parameters (kv pairs) from there
            var structData = entry.StructuredData;
            if (structData != null)
            {
                var structDataParams = structData.SelectMany(de => de.Value).ToList();
                allParams.AddRange(structDataParams);
            }

            // now convert to dictionary; first group param values by param name
            var grouped = allParams.GroupBy(p => p.Name, p => p.Value).ToDictionary(g => g.Key, g => g.ToArray());
            // copy to final dictionary - keep multiple values as an array; 
            // for single value put it as a single object (not object[1]); - except for IP addresses - these are always arrays, even if there's only one
            entry.AllData.Clear();
            foreach (var kv in grouped)
            {
                if (kv.Value.Length > 1 || kv.Key == "IPv4" || kv.Key == "IPv6")
                {
                    entry.AllData[kv.Key] = kv.Value; //array
                }
                else
                    entry.AllData[kv.Key] = kv.Value[0]; //single value
            }
        }
    }
}
