// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Diagnostics.Tracing;

    public class RxKqlEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Diagnostic = (EventKeywords)1;
            public const EventKeywords Perf = (EventKeywords)2;
        }

        [Event(5001, Message = "Application Exception: {0}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        public void LogException(string message)
        {
            if (IsEnabled())
            {
                WriteEvent(5001, message);
            }
        }

        [Event(5002, Message = "Log DateTime Out of Range Exception: {0} {1} {2} {3} {4} {5}", Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        public void LogDateTimeOutofRangeException(int year, int month, int day, int hour, int minute, int second, char durationUnit)
        {
            if (IsEnabled())
            {
                WriteEvent(5002, year, month, day, hour, minute, second, durationUnit);
            }
        }

        public static RxKqlEventSource Log = new RxKqlEventSource();
    }
}
