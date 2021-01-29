// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("array_length")]
    public class ArrayLengthFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ArrayLengthFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);

            if (value.IsArray())
            {
                object[] res = value as object[];
                return res.Length;
            }

            if (value.IsDynamicObject())
            {
                var temp = (IDictionary<string, object>) value;
                return temp.Keys.Count;
            }

            if (value.IsGenericCollectionObject())
            {
                return 2;
            }

            return -1;
        }
    }
}