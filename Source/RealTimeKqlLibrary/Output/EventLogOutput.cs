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
        private readonly string _logName;
        private readonly string _source;
        private readonly EventLog _eventLog;
        private readonly bool _friendlyFormat;
        private readonly string _delimiter;
        private bool _error;

        private readonly BaseLogger _logger;

        public EventLogOutput(BaseLogger logger, string logName, string sourceName, bool friendlyFormat = false)
        {
            _logger = logger;
            _logName = logName;
            _source = sourceName;
            _eventLog = new EventLog(logName);
            _friendlyFormat = friendlyFormat;
            _delimiter = $": ";
            _error = false;

            try
            {
                // Create the source, if it does not already exist.
                if (!EventLog.SourceExists(_source))
                {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, will have to sleep
                    //before writing events to log
                    EventLog.CreateEventSource(_source, _logName);
                    _logger.Log(LogLevel.INFORMATION, "CreatedEventSource");
                    _logger.Log(LogLevel.INFORMATION, "Sleeping to let the machine catch up...");
                    Thread.Sleep(10000);
                }

                // Setting event source
                _eventLog.Source = _source;
            }
            catch(Exception ex)
            {
                OutputError(ex);
            }

        }
        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_error) return;

            try
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
            }
            catch(Exception ex)
            {
                OutputError(ex);
            }
        }

        public void OutputCompleted()
        {
            _logger.Log(LogLevel.INFORMATION, "Stopping RealTimeKql...");
            _eventLog.Close();
        }

        public void OutputError(Exception ex)
        {
            _error = true;
            _logger.Log(LogLevel.ERROR, ex);
        }

        public void Stop()
        {
            _logger.Log(LogLevel.INFORMATION, "\nCompleted!\nThank you for using RealTimeKql!");
        }
    }
}
