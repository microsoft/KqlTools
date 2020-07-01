// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("trim")]
    public class TrimFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public TrimFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var originalValue = Arguments[1].GetValue(evt);
            var regex = Arguments[0].GetValue(evt);

            // If the string starts or ends with a matching string of the RegEx pattern, remove it.
            return RxKqlCommonFunctions.TrimStartWithRegex(regex.ToString(), RxKqlCommonFunctions.TrimEndWithRegex(regex.ToString(), originalValue.ToString()));
        }
    }
}