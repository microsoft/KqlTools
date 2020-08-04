// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft                             *
// *                                                       *
// ********************************************************/

namespace Microsoft.Syslog
{
    using System;
    using Microsoft.Syslog.Model;

    public class SyslogEntryEventArgs : EventArgs
    {
        public ServerSyslogEntry ServerEntry { get; }

        internal SyslogEntryEventArgs(ServerSyslogEntry entry)
        {
            ServerEntry = entry; 
        }
    }


    public class SyslogErrorEventArgs: EventArgs
    {
        public Exception Error { get; }

        internal SyslogErrorEventArgs(Exception error)
        {
            Error = error; 
        }
    }
}
