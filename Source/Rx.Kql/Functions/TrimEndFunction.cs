// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Text.RegularExpressions;

    [Description("trim_end")]
    public class TrimEndFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public TrimEndFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var originalValue = Arguments[1].GetValue(evt);
            var regex = Arguments[0].GetValue(evt);

            // If the string ends with a matching string of the RegEx pattern, remove it.
            return RxKqlCommonFunctions.TrimEndWithRegex(regex.ToString(), originalValue.ToString());
        }
    }
}