// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Dynamic;

    [Description("pack")]
    public class PackFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public PackFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var returnDictionary = new ExpandoObject() as IDictionary<string, object>;
            int iterations = Arguments.Count / 2;

            for (int i = 0; i < iterations; i++)
            {
                string argName = Arguments[i * 2].GetValue(evt).ToString();

                // Get the value.
                object argValue = Arguments[(i * 2) + 1].GetValue(evt);

                // Make sure the field name contains no single or double quotes
                returnDictionary.Add(argName, argValue);
            }

            return returnDictionary;
        }
    }
}