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
    using System.Reactive.Kql.Expressions;

    [Description("case")]
    public class CaseFunction : ScalarFunction
    {
        /// <summary>
        ///     Empty constructor supporting Serialization/Deserialization.  DO NOT REMOVE
        /// </summary>
        public CaseFunction()
        {
        }

        public override object GetValue(IDictionary<string, object> evt)
        {
            if (Arguments.Count % 2 == 0)
            {
                throw new InvalidArgumentException(
                    "Case Function: No Else part present");
            }

            for (int i = 0; i < Arguments.Count - 1; i = i + 2)
            {
                if (Arguments[i] is ScalarConst scalarConst &&
                    scalarConst.Value.GetType() == typeof(bool) &&
                    (bool)scalarConst.Value == true)
                {
                    return FetchValue(Arguments[i + 1], evt); 
                }
                else if (Arguments[i] is ScalarFunction scalarFunction &&
                    (bool)scalarFunction.GetValue(evt) == true)
                {
                    return FetchValue(Arguments[i + 1], evt);
                }
                else if (Arguments[i] is ComparisonExpression expression &&
                         (bool)expression.GetValue(evt) == true)
                {
                    return FetchValue(Arguments[i + 1], evt);
                }
                else if (Arguments[i] is ScalarValue value &&
                         (bool)value.GetValue(evt) == true)
                {
                    return FetchValue(Arguments[i + 1], evt);
                }
                else if (!(Arguments[i] is ScalarConst) &&
                         !(Arguments[i] is ScalarFunction) &&
                         !(Arguments[i] is ScalarValue) &&
                         !(Arguments[i] is ComparisonExpression))
                {
                    throw new InvalidArgumentException(
                        "Case Function: Invalid Comparision Expression");
                }
            }

            return FetchValue(Arguments[Arguments.Count - 1], evt);
        }

        private static object FetchValue(ScalarValue scalarValue, IDictionary<string, object> evt)
        {
            if (scalarValue is ScalarConst scalarConst)
            {
                return scalarConst.Value;
            }
            else if (scalarValue is ScalarFunction scalarFunction)
            {
                return scalarFunction.GetValue(evt);
            }
            else
            {
                throw new InvalidArgumentException(
                    "Case Function: Scalar Constant or Scalar Function Expected");
            }
        }
    }
}