// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("format_datetime")]
    public class FormatDateTimeFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public FormatDateTimeFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            DateTime dateValueToFormat = (DateTime)Arguments[0].GetValue(evt);
            string dateFormatToUse = Arguments[1].GetValue(evt).ToString();

            string retVal = dateValueToFormat.ToString(dateFormatToUse);
            return retVal;
        }
    }
}
