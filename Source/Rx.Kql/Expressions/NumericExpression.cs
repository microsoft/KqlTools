// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql.Expressions
{
    using System;
    using System.Collections.Generic;

    [Operator("-", "+", "*", "/")]
    public class NumericExpression : BinaryExpression
    {
        public override object GetValue(IDictionary<string, object> evt)
        {
            var leftValue = RxKqlCommonFunctions.ParseNumericExpressionItem(Left.GetValue(evt).ToString());
            var rightValue = RxKqlCommonFunctions.ParseNumericExpressionItem(Right.GetValue(evt).ToString());

            if (leftValue is int || leftValue is long || leftValue is double)
            {
                var num1 = Convert.ToDouble(leftValue);
                var num2 = Convert.ToDouble(rightValue);

                switch (Operator)
                {
                    case "-":
                        return num1 - num2;

                    case "+":
                        return num1 + num2;

                    case "*":
                        return num1 * num2;

                    case "/":
                        return num1 / num2;

                    default:
                        throw new InvalidOperationException($"Invalid Operator: {Operator}");
                }
            }
            else if (leftValue is DateTime)
            {
                var date1 = Convert.ToDateTime(leftValue);
                var date2 = Convert.ToDateTime(rightValue);

                switch (Operator)
                {
                    case "-":
                        return date1 - date2;

                    case "+":
                        return new DateTime(date1.Ticks + date2.Ticks);

                    default:
                        throw new InvalidOperationException($"Invalid Operator: {Operator}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Invalid Numeric type: {leftValue.GetType()}");
            }
        }
    }
}
