// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace RealTimeKql
{
    using Microsoft.Syslog.Model;
    using Newtonsoft.Json;
    using System.IO;

    public class SyslogFilter
    {
        public string[] Keywords;

        public SyslogFilter(string fileName = "filter.json")
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!File.Exists(fileName))
                {
                    throw new FileNotFoundException($"Filter file {fileName} not found.");
                }

                var json = File.ReadAllText(fileName);
                Keywords = JsonConvert.DeserializeObject<string[]>(json);
            }
        }

        public bool Allow(ServerSyslogEntry serverEntry)
        {
            if (Keywords == null || Keywords.Length == 0)
            {
                return true;
            }

            var text = serverEntry.Payload;
            for (int i = 0; i < Keywords.Length; i++)
            {
                if (text.Contains(Keywords[i]))
                {
                    return false;
                }
            }

            return true; 
        }

    }
}
