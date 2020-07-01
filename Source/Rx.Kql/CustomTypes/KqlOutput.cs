// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.CustomTypes
{
    using System.Collections.Generic;

    public class KqlOutput
    {
        public string Query { get; set; }

        public string Comment { get; set; }

        public IDictionary<string, object> Output { get; set; }

        public KqlQuery KqlQuery { get; set; }
    }
}