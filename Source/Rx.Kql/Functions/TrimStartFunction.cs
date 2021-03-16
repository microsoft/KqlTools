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
    using System.Text.RegularExpressions;

    [Description("trim_start")]
    public class TrimStartFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public TrimStartFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var originalValue = Arguments[1].GetValue(evt);
            var regex = Arguments[0].GetValue(evt);

            // If the string starts with a matching string of the RegEx pattern, remove it.
            return RxKqlCommonFunctions.TrimStartWithRegex(regex.ToString(), originalValue.ToString());
        }
    }
}