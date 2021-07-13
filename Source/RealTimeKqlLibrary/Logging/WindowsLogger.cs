using System;
using System.Diagnostics;
using System.Threading;

namespace RealTimeKqlLibrary
{
    public class WindowsLogger : BaseLogger
    {
        private readonly EventLog _eventLog;

        // TODO: add optional logging to kusto

        public WindowsLogger(string source, string log) : base()
        {
            // create event log source and log if it does not exist
            _eventLog = new EventLog(log);
            if(!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, log);
                Thread.Sleep(10000);   
            }
            _eventLog.Source = source;
        }

        public override void Log(LogLevel logLevel, object payload)
        {
            // Check if log level is enabled
            if (!IsEnabled(logLevel)) return;

            // Log to console
            Console.WriteLine(payload);

            // Log to event log
            int eventId;
            EventLogEntryType eventLogEntryType;

            switch(logLevel)
            {
                case LogLevel.ERROR:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Error;
                    break;
                case LogLevel.DEBUG:
                    eventId = (int)logLevel;
                    eventLogEntryType = EventLogEntryType.Information;
                    break;
                default:
                    eventId = 0;
                    eventLogEntryType = EventLogEntryType.Information;
                    break;
            }

            _eventLog.WriteEvent(new EventInstance(eventId, 0, eventLogEntryType), payload);
        }
    }
}
