// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using ComponentModel;
    using System.Collections.Generic;

    [Description("isnull")]
    public class IsNullFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public IsNullFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var arg = Arguments[0].GetValue(evt);
            return arg == null || (arg is string && string.IsNullOrWhiteSpace((string)arg));
        }
    }
}
