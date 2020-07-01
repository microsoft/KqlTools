// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("totimespan")]
    public class ToTimeSpanFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToTimeSpanFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            TimeSpan result;
            if (!TimeSpan.TryParse(Arguments[0].GetValue(evt).ToString().Trim(' ', '"'), out result))
            {
                throw new ArgumentException("Parameter of totimespan function is expected to be in 'totimespan(\"0.00:01:00\")' format.");
            }

            return result;
        }
    }
}