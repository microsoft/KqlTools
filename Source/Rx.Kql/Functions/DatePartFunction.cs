// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;

    [Description("datepart")]
    public class DatePartFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public DatePartFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            long result = 0;

            var dict = (IDictionary<string, object>) evt;

            string datepartName = Arguments[0].GetValue(evt).ToString();
            var targetDateTime = (DateTime)Arguments[1].GetValue(evt);

            result = RxKqlCommonFunctions.GetDatePart(datepartName, targetDateTime);

            return result;
        }
    }
}