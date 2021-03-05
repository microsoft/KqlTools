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
    using Text;

    [Description("bin")]
    public class BinFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public BinFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string value = Arguments[0].GetValue(evt).ToString();
            string roundTo = Arguments[1].GetValue(evt).ToString();

            return (Convert.ToInt64(Convert.ToInt64(value) / Convert.ToInt64(roundTo)) * Convert.ToInt64(roundTo));
        }
    }
}