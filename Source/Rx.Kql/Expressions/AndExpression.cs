// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System.Collections.Generic;
    using System.Reactive.Kql.ExceptionTypes;

    [Operator("and")]
    public class AndExpression : BinaryExpression
    {
        public AndExpression()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            var left = Left.GetValue(evt);
            var right = Right.GetValue(evt);
            if (!RxKqlCommonFunctions.TryConvert<bool>(left, out var leftBool))
            {
                throw new EvaluationTypeMismatchException($"Cannot convert value {left} to bool");
            }
            if (!RxKqlCommonFunctions.TryConvert<bool>(right, out var rightBool))
            {
                throw new EvaluationTypeMismatchException($"Cannot convert value {right} to bool");
            }

            return leftBool && rightBool;
        }
    }
}