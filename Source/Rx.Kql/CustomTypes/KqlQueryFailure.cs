// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.CustomTypes
{
    using System.Collections.Generic;

    public class KqlQueryFailure
    {
        public KqlQuery KqlQuery { get; set; }

        public Exception Exception { get; set; }

        public IDictionary<string, object> EventDictionary { get; set; }
    }
}