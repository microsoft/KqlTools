// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Functions
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Text.RegularExpressions;

    [Description("replace")]
    public class ReplaceFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ReplaceFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            string returnValue, inputString, replaceString, regexPattern;

            if (Arguments.Count < 3)
            {
                throw new InvalidArgumentException("Missing Parameters");
            }
            else
            {
                regexPattern = Arguments[0].GetValue(evt).ToString();
                replaceString = Arguments[1].GetValue(evt).ToString();
                inputString = Arguments[2].GetValue(evt).ToString();

                Regex regex = new Regex(regexPattern);

                returnValue = regex.Replace(inputString, replaceString);
            }

            return returnValue;
        }
    }
}