// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;

    internal class ProviderStringCache
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, DisplayNames>> _cache = new ConcurrentDictionary<string, ConcurrentDictionary<int, DisplayNames>>();

        public void Lookup(EventRecord evt, out string level, out string task, out string opcode, out string keywords)
        {
            var providerId = evt.ProviderId.ToString();
            if (providerId == null)
            {
                providerId = evt.ProviderName;
            }

            ConcurrentDictionary<int, DisplayNames> events = null;
            if (!_cache.TryGetValue(providerId, out events))
            {
                events = new ConcurrentDictionary<int, DisplayNames>();
                _cache.TryAdd(providerId, events);
            }

            DisplayNames names = null;
            if (!events.TryGetValue(evt.Id, out names))
            {
                names = null;
                try
                {
                    names = new DisplayNames
                    {
                        Level = ValueOrEmpty(evt.LevelDisplayName),
                        Task = ValueOrEmpty(evt.TaskDisplayName),
                        Opcode = ValueOrEmpty(evt.OpcodeDisplayName),
                        Keywords = ValueOrEmpty(string.Join(",", evt.KeywordsDisplayNames))
                    };
                }
                catch (Exception)
                {
                    names = new DisplayNames
                    {
                        Level = ValueOrEmpty(evt.Level.ToString()),
                        Task = ValueOrEmpty(evt.Task.ToString()),
                        Opcode = ValueOrEmpty(evt.Opcode.ToString())
                    };
                }

                events.TryAdd(evt.Id, names);
            }

            level = names.Level;
            task = names.Task;
            opcode = names.Opcode;
            keywords = names.Keywords;
        }

        private string ValueOrEmpty(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value;
        }

        private class DisplayNames
        {
            public string Level;
            public string Opcode;
            public string Task;
            public string Keywords;
        }
    }
}