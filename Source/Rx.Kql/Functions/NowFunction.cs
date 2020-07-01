// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("now")]
    public class NowFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public NowFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            return now;
        }
    }
}