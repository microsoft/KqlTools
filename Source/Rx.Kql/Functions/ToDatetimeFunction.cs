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

    [Description("todatetime")]
    public class ToDatetimeFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public ToDatetimeFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var argument = Arguments[0].GetValue(evt).ToString().Trim('"', ' ', '\'');
            DateTimeOffset result;
            if (!DateTimeParser.TryParseDatetime(argument, out result))
            {
                throw new ArgumentException(
                    $"Parameter of todatetime function is expected to be in one of the following formats.\r\n{string.Join(", ", RxKqlCommonFunctions.SupportedDateTimeFormatStrings)}");
            }

            return result;
        }
    }
}