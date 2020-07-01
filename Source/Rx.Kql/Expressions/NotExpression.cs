// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Kql.ExceptionTypes;

    [Description("not")]
    public class NotExpression : ScalarFunction
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var value = Arguments[0].GetValue(evt);
            if (!RxKqlCommonFunctions.TryConvert<bool>(value, out var result))
            {
                throw new EvaluationTypeMismatchException($"Cannot convert value {value} to bool");
            }
            return !result;
        }
    }
}