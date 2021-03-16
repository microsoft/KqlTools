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

    [Description("packarray")]
    public class PackArrayFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public PackArrayFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            int iterations = Arguments.Count;
            var returnDictionary = new object[iterations];

            for (int i = 0; i < iterations; i++)
            {
                var argValue = Arguments[i].GetValue(evt);

                returnDictionary[i] = argValue;
            }

            return returnDictionary;
        }
    }
}