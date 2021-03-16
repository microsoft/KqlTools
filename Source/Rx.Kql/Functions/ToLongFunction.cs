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

    [Description("tolong")]
    public class ToLongFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToLongFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);

            if (long.TryParse(value.ToString(), out long number))
            {
                return number;
            }
            else
            {
                return null;
            }
        }
    }
}