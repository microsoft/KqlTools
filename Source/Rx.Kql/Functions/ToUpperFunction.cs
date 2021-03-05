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

    [Description("toupper")]
    public class ToUpperFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToUpperFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var dict = (IDictionary<string, object>)evt;
            var value = Arguments[0].GetValue(evt);
            return value.ToString().ToUpper();
        }
    }
}