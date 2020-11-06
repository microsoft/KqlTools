// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests
{
    using System;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reactive.Kql;
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reactive.Kql.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExpressionOverloadTests : TestBase
    {
        [TestMethod]
        public void Aris_BasicExpressionTree()
        {
            dynamic evt = new ExpandoObject();
            evt.Var1 = 123;
            evt.Var2 = 456;
            evt.Var3 = 789;
            evt.Var4 = 101112;

            WhereOperator whereOperator = new WhereOperator();

            List<ScalarValue> orExpressions = new List<ScalarValue>();
            orExpressions.Add(new EqualsExpression
            {
                Left = new ScalarProperty("Var1"),
                Operator = "==",
                Right = new ScalarConst { Value = 123 }
            });
            orExpressions.Add(new EqualsExpression
            {
                Left = new ScalarProperty("Var2"),
                Operator = "==",
                Right = new ScalarConst { Value = 456 }
            });
            var compositeExpression = new OrExpression
            {
                Left = orExpressions[0],
                Right = orExpressions[1]
            };

            // Add a conditional AND expression to the embedded OR Expression
            var basicExpression = new EqualsExpression
            {
                Left = new ScalarProperty("Var3"),
                Operator = "==",
                Right = new ScalarConst { Value = 789 }
            };
            var aggregateExpression = new AndExpression
            {
                Left = compositeExpression,
                Right = basicExpression
            };

            // Add a conditional AND expression to the embedded Composite Expression
            var newOuterExpressionExpression = new EqualsExpression
            {
                Left = new ScalarProperty("Var4"),
                Operator = "==",
                Right = new ScalarConst { Value = 101112 }
            };
            var finalExpression = new AndExpression
            { 
                Left = aggregateExpression,
                Right = newOuterExpressionExpression
            };

            // Evaluate the current Expression for TRUE
            whereOperator.Expression = finalExpression;
            TestWhere(evt, whereOperator, true);

            // Evaluate the current Expression for FALSE
            evt = new ExpandoObject();
            evt.Var1 = 123;
            evt.Var2 = 111; // Evaluate false
            evt.Var3 = 222; // Evaluate false
            evt.Var4 = 101112;
            TestWhere(evt, whereOperator, false);

            // Evaluate the current Expression for FALSE
            evt = new ExpandoObject();
            evt.Var1 = 123;
            evt.Var2 = 111; // Evaluate true
            evt.Var3 = 789;
            evt.Var4 = 101112;
            TestWhere(evt, whereOperator, true);

            // Dehydrate
            string jsonWhere = RxKqlCommonFunctions.ToJson(whereOperator.Expression);

            // Hydrate
            var booleanExpression = RxKqlCommonFunctions.ToBooleanExpression(jsonWhere);

            // Compare the expected result (e.g. hardcoded in the test)
            // with the actual value
            Assert.AreEqual(whereOperator.Expression, finalExpression);
        }

        [TestMethod]
        public void Aris_BasicNumericComparisonTree()
        {
            dynamic evt = new ExpandoObject();

            WhereOperator whereOperatorHigh = new WhereOperator("sum_flow_count_high > 1000");
            WhereOperator whereOperatorLow = new WhereOperator("sum_flow_count_low > 1000");

            // Dehydrate
            string jsonWhere = RxKqlCommonFunctions.ToJson(whereOperatorHigh.Expression);

            // Hydrate
            var booleanExpression = RxKqlCommonFunctions.ToBooleanExpression(jsonWhere);

            // Evaluate the current Expression 
            evt = new ExpandoObject();

            try
            {
                // purposely add a value as a string and compare it to an integer, to throw a specific Exception Type
                evt.sum_flow_count_high = "101112";
                TestWhere(evt, whereOperatorHigh, true);
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();
                Assert.AreEqual(exceptionType, typeof(EvaluationTypeMismatchException));
            }

            evt.sum_flow_count_low = 999;
            TestWhere(evt, whereOperatorLow, false);
        }

        [TestMethod]
        public void Aris_PackArrayTest()
        {
            dynamic evt = new ExpandoObject();

            WhereOperator whereOperatorHigh = new WhereOperator("sum_flow_count_low > 1000");
            WhereOperator whereOperatorLow = new WhereOperator("sum_flow_count_low > 1000");

            // Dehydrate
            string jsonWhere = RxKqlCommonFunctions.ToJson(whereOperatorHigh.Expression);

            // Hydrate
            var booleanExpression = RxKqlCommonFunctions.ToBooleanExpression(jsonWhere);

            // Evaluate the current Expression 
            evt = new ExpandoObject();

            try
            {
                // purposely add a value as a string and compare it to an integer, to throw a specific Exception Type
                evt.sum_flow_count_high = "101112";
                TestWhere(evt, whereOperatorHigh, true);
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();
                Assert.AreEqual(exceptionType, typeof(EvaluationTypeMismatchException));
            }

            evt.sum_flow_count_low = 999;
            TestWhere(evt, whereOperatorLow, false);
        }
    }
}