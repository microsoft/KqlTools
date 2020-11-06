// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.EventTypes
{
    using System.Collections.Generic;

    /// <summary>
    /// Event arguments for a KQL query that failed execution. 
    /// </summary>
    public class KqlQueryFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The time that the KQL query failed execution.
        /// </summary>
        public DateTime FailureDateTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Message to send with the event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Comment describing the query.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The KQL query as a string.
        /// </summary>
        public string Query { get; set; }
        
        /// <summary>
        /// The System.Exception that was raised as a result of the failure.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The set of events associated with the query failure.
        /// </summary>
        public IDictionary<string, object> EventDictionary { get; set; }
    }
}