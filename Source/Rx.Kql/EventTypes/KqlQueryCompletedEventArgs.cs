// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.EventTypes
{
    /// <summary>
    /// Event arguments for a completed KQL query. 
    /// </summary>
    public class KqlQueryCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// The time that the KQL query completed execution.
        /// </summary>
        public DateTime CompletionDateTime { get; set; } = DateTime.UtcNow;

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
    }
}