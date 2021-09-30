using System;
using System.Diagnostics;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class WindowsLogger : BaseLogger
    {
        private readonly string _source;
        private readonly string _log;
        private EventLog _eventLog;

        // TODO: add optional logging to kusto

        public WindowsLogger(string source, string log)
        {
            _source = source;
            _log = log;
        }

        public override bool Setup()
        {
            try
            {
                // create event log source and log if it does not exist
                _eventLog = new EventLog(_log);
                if (!EventLog.SourceExists(_source))
                {
                    EventLog.CreateEventSource(_source, _log);
                    Thread.Sleep(10000);
                }
                _eventLog.Source = _source;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

            return true;
        }

        public override void Log(LogLevel logLevel, object payload)
        {
            // Check if log level is enabled
            if (!IsEnabled(logLevel) || logLevel == LogLevel.NONE) return;

            // Log to console
            Console.WriteLine(payload);

            // Log to event log
            int eventId;
            EventLogEntryType eventLogEntryType;

            switch(logLevel)
            {
                case LogLevel.CRITICAL:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Error;
                    break;
                case LogLevel.ERROR:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Error;
                    break;
                case LogLevel.WARNING:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Warning;
                    break;
                case LogLevel.INFORMATION:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Information;
                    break;
                default:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Information;
                    break;
            }

            _eventLog.WriteEvent(new EventInstance(eventId, 0, eventLogEntryType), payload);
        }
    }
}
