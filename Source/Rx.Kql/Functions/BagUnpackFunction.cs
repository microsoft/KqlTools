// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Kql.ExceptionTypes;

    [Description("bag_unpack")]
    public class BagUnpackFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public BagUnpackFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var dict = evt;
            var argValue = new object();
            var args = Arguments.ToList();
            var appendedFieldPrefix = args.Count == 2 ? args[1].GetValue(evt).ToString().TrimEdges(new List<string> { "\"", "'" }, StringComparison.InvariantCultureIgnoreCase) : string.Empty;

            if (!(args[0] is ScalarProperty))
            {
                throw new EvaluationTypeMismatchException($"{args[0]} is not a property");
            }
            var unpackedDictionary = (IDictionary<string, object>)evt[((ScalarProperty)args[0]).Value.ToString()];
            foreach (string unpackedName in unpackedDictionary.Keys)
            {
                // As per Kusto functionality, if extending an existing field in a query
                // the value of the extended field overrides the object value
                string newFieldName = $"{appendedFieldPrefix}{unpackedName}";

                if (evt.ContainsKey(newFieldName))
                {
                    evt[newFieldName] = unpackedDictionary[unpackedName];
                }
                else
                {
                    evt.Add(newFieldName, unpackedDictionary[unpackedName]);
                }
            }

            return evt;
        }
    }
}