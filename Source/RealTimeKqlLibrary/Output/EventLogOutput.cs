using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Kql.CustomTypes;
using Newtonsoft.Json;

namespace RealTimeKqlLibrary
{
    public class EventLogOutput : IOutput
    {
        private readonly string _source;
        private readonly EventLog _eventLog;
        private readonly bool _friendlyFormat;
        private readonly string _delimiter;

        private readonly BaseLogger _logger;

        public EventLogOutput(BaseLogger logger, string logName, bool friendlyFormat = false)
        {
            _logger = logger;
            _source = "RealTimeKql";
            _eventLog = new EventLog(logName);
            _friendlyFormat = friendlyFormat;
            _delimiter = $": ";

            // Create the source, if it does not already exist.
            if (!EventLog.SourceExists(_source))
            {
                //An event log source should not be created and immediately used.
                //There is a latency time to enable the source, will have to sleep
                //before writing events to log
                EventLog.CreateEventSource(_source, logName);
                _logger.Log(LogLevel.INFORMATION, "CreatedEventSource");
                _logger.Log(LogLevel.INFORMATION, "Sleeping to let the machine catch up...");
                Thread.Sleep(10000);
            }

            // Setting event source
            _eventLog.Source = _source;
        }
        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            // Setting event data & writing to log
            if (_friendlyFormat)
            {
                // Writing out string representation of data
                var values = new List<object>();
                foreach (var kv in obj)
                {
                    if (kv.Value.GetType() == typeof(System.Dynamic.ExpandoObject))
                    {
                        foreach (var pair in (IDictionary<string, object>)kv.Value)
                        {
                            values.Add($"{pair.Key}{_delimiter}{pair.Value}");
                        }
                    }
                    else
                    {
                        values.Add($"{kv.Key}{_delimiter}{kv.Value}");
                    }
                }

                // Writing event to log
                _eventLog.WriteEvent(new EventInstance(0, 0, EventLogEntryType.Information), values.ToArray());
            }
            else
            {
                // Serializing data
                var json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });


                // Writing event to log
                _eventLog.WriteEvent(new EventInstance(0, 0, EventLogEntryType.Information), json);
            }

            _logger.Log(LogLevel.VERBOSE, "Writing new entry...");
        }

        public void OutputCompleted()
        {
            _eventLog.Close();
        }

        public void OutputError(Exception ex)
        {
            _eventLog.WriteEvent(new EventInstance(0, 0, EventLogEntryType.Error), ex.ToString());
        }

        public void Stop()
        {
            _logger.Log(LogLevel.INFORMATION, "\nCompleted!\nThank you for using RealTimeKql!");
        }
    }
}
