// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.CustomTypes
{
    using System.Collections.Generic;

    /// <summary>
    /// Class that defines the output structure of KQL queries. 
    /// </summary>
    public class KqlOutput
    {
        /// <summary>
        /// The KQL query as a string.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// Comment describing the KQL query.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The output of the KQL query as an IDictionary.
        /// </summary>
        public IDictionary<string, object> Output { get; set; }

        /// <summary>
        /// The KQL query and associated metadata as a KqlQuery instance.
        /// </summary>
        public KqlQuery KqlQuery { get; set; }
    }
}