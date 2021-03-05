// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("isempty")]
    class IsEmptyFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public IsEmptyFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var arg = Arguments[0].GetValue(evt);
            return arg == null || (arg is string && string.IsNullOrEmpty((string)arg));
        }
    }
}
