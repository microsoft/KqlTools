// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("tolower")]
    public class ToLowerFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToLowerFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var dict = (IDictionary<string, object>)evt;
            var value = Arguments[0].GetValue(evt);
            return value.ToString().ToLower();
        }
    }
}