// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System.Text;
using Newtonsoft.Json;

namespace System.Reactive.Kql
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("tostring")]
    public class ToStringFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToStringFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);

            // Support Generic lists 
            if (value.IsGenericList())
            {
                return JsonConvert.SerializeObject(value);
            }

            return Arguments[0].GetValue(evt).ToString();
        }
    }
}