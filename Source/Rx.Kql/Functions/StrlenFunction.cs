// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("strlen")]
    public class StrlenFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public StrlenFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var dict = (IDictionary<string, object>)evt;
            var arg = Arguments[0].GetValue(evt).ToString();
            return arg.Length;
        }
    }
}