// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("isnotempty")]
    public class IsNotEmptyFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public IsNotEmptyFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            if (Arguments.Count < 1)
            {
                return false;
            }

            var arg = Arguments[0].GetValue(evt);
            return !(arg == null || (arg is string && string.IsNullOrEmpty((string)arg)));
        }
    }
}