// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace WinLog.LogHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;

    /// <summary>
    ///     EvtxEnumerable class enabling reading from log files and from operating system logs on the machine.
    /// </summary>
    public static class EvtxEnumerable
    {
        /// <summary>
        ///     Read an Windows OS Event Log, from an optional bookmark
        /// </summary>
        /// <param name="logFile">the path to the event log file</param>
        /// <param name="bookmark">the eventbookmark (optional)</param>
        /// <returns></returns>
        public static IEnumerable<EventRecord> ReadWindowsLog(string logName, EventBookmark bookmark = null)
        {
            long eventCount = 0; // for debugging

            var query = new EventLogQuery(logName, PathType.LogName, "*");
            using (var reader = new EventLogReader(query, bookmark))
            {
                for (;;)
                {
                    var record = reader.ReadEvent();
                    if (record == null)
                    {
                        yield break;
                    }

                    eventCount++;
                    yield return record;
                }
            }
        }

        /// <summary>
        ///     Read an Windows Event Log file (EXTX), from an optional bookmark
        /// </summary>
        /// <param name="logFile">the path to the event log file</param>
        /// <param name="bookmark">the eventbookmark (optional)</param>
        /// <returns></returns>
        public static IEnumerable<EventRecord> ReadEvtxFile(string logFile, EventBookmark bookmark = null)
        {
            long eventCount = 0; // for debugging
            var query = new EventLogQuery(logFile, PathType.FilePath, "*");
            using (var reader = new EventLogReader(query, bookmark))
            {
                for (;;)
                {
                    var record = reader.ReadEvent();
                    if (record == null)
                    {
                        yield break;
                    }

                    eventCount++;
                    yield return record;
                }
            }
        }
    }
}