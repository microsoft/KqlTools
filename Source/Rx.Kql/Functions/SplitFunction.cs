// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("split")]
    public class SplitFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public SplitFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string returnValue = string.Empty;
            string stringToSplit = Arguments[0].GetValue(evt).ToString();
            string splitter = Arguments[1].GetValue(evt).ToString();

            if (!string.IsNullOrEmpty(stringToSplit) &&
                !string.IsNullOrEmpty(splitter))
            {
                var retVal = stringToSplit.Split(new string[] { splitter }, StringSplitOptions.None);

                if (Arguments.Count == 3 &&
                    int.TryParse(Arguments[2].GetValue(evt).ToString(), out int requestedIndex) &&
                    retVal.Length > requestedIndex)
                {
                    return retVal[requestedIndex];
                }

                return retVal;
            }

            return string.Empty;
        }
    }
}
