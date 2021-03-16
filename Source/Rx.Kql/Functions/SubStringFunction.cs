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
    using System.Reactive.Kql.ExceptionTypes;

    [Description("substring")]
    public class SubStringFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public SubStringFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string returnValue = string.Empty;
            int strLength = 0;
            string strToCheck = Arguments[0].GetValue(evt).ToString();

            if (Arguments.Count == 3)
            {
                if (!int.TryParse(Arguments[2].GetValue(evt).ToString(), out strLength))
                {
                    throw new InvalidArgumentException("Invalid Parameter");
                }
            }

            if (strToCheck.Length > 0 && int.TryParse(Arguments[1].GetValue(evt).ToString(), out int startIndex))
            {
                if (Arguments.Count == 3 && 
                    startIndex < strToCheck.Length &&
                    startIndex + strLength < strToCheck.Length)
                {
                    returnValue = strToCheck.Substring(startIndex, strLength);
                }
                else if (Arguments.Count == 2)
                {
                    returnValue = strToCheck.Substring(startIndex);
                }
            }

            return returnValue;
        }
    }
}