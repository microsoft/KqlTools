// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.CustomTypes
{
    using System.Collections.Generic;

    /// <summary>
    /// Class that defines the metadata for KQL queries that failed execution. 
    /// </summary>
    public class KqlQueryFailure
    {
        /// <summary>
        /// The KQL query and associated metadata as a KqlQuery instance.
        /// </summary>
        public KqlQuery KqlQuery { get; set; }

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