// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("time")]
    public class TimeFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public TimeFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            if (!DateTimeParser.TryParseAgoValue(Arguments[0].GetValue(evt).ToString(), out TimeSpan result))
            {
                throw new ArgumentException("Parameter of time function is expected to be in timespan format.");
            }

            return result;
        }
    }
}