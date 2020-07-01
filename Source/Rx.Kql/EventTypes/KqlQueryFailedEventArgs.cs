// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.EventTypes
{
    using System.Collections.Generic;

    public class KqlQueryFailedEventArgs : EventArgs
    {
        public DateTime FailureDateTime { get; set; } = DateTime.UtcNow;

        public string Message { get; set; }

        public string Comment { get; set; }

        public string Query { get; set; }

        public Exception Exception { get; set; }

        public IDictionary<string, object> EventDictionary { get; set; }
    }
}