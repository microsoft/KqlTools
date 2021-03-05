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

    [Description("toint")]
    public class ToIntFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToIntFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);

            if (int.TryParse(value.ToString(), out int number))
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