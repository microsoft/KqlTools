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
        private readonly int _eventId;
        private bool _firstEntry;
        private bool _error;

        private readonly BaseLogger _logger;

        public EventLogOutput(BaseLogger logger, string logName, string sourceName)
        {
            _logger = logger;
            _logName = logName;
            _source = sourceName;
            _eventLog = new EventLog(logName);
            _eventId = 6;
            _firstEntry = true;
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

            if(_firstEntry)
            {
                _logger.Log(LogLevel.INFORMATION, "Writing events to log...");
                _firstEntry = false;
            }

            try
            {
                // Serializing data
                var json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });


                // Writing event to log
                _eventLog.WriteEvent(new EventInstance(_eventId, 0, EventLogEntryType.Information), json);
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
