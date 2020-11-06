// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Kql;
    using System.Reactive.Subjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TestBase
    {
        protected void TestWhere(dynamic evt, string expression, bool expected)
        {
            // If the following line passes, it usually means parsing is ok
            var where = new WhereOperator(expression);
            var exp = where;

            // For operators not present in Rx.Net there is runtime implementation
            bool actual = exp.Evaluate(evt);

            // Compare the expected result (e.g. hardcoded in the test)
            // with the actual value
            Assert.AreEqual(expected, actual);
        }

        protected void TestWhere(dynamic evt, WhereOperator whereOperator, bool expected)
        {
            var exp = whereOperator;

            // For operators not present in Rx.Net there is runtime implementation
            bool actual = exp.Evaluate(evt);

            // Compare the expected result (e.g. hardcoded in the test)
            // with the actual value
            Assert.AreEqual(expected, actual);
        }

        protected List<IDictionary<string, object>> RunAtomicQuery(dynamic evt, string query)
        {
            var subject = new Subject<IDictionary<string, object>>();
            var result = new List<IDictionary<string, object>>();

            subject.KustoQuery(query)
                .Subscribe(e =>
                {
                    result.Add(e);
                });

            subject.OnNext(evt);

            return result;
        }

        protected List<dynamic> RunAtomicQueryWithKqlNode(dynamic eventDynamic, string query)
        {
            var result = new List<dynamic>();
            KqlNode node = new KqlNode();

            node.AddKqlQuery(new KqlQuery
            {
                Comment = "TestQuery",
                Query = query
            });

            // Subscribe to the sucessful detections.
            node.Subscribe(evt => { result.Add(evt.Output); });

            // Add the detections.
            node.OnNext((IDictionary<string, object>) eventDynamic);

            return result;
        }
    }
}