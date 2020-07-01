// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("tobool")]
    public class ToBoolFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToBoolFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            bool.TryParse(Arguments[0].GetValue(evt).ToString(), out bool retVal);

            return retVal;
        }
    }
}