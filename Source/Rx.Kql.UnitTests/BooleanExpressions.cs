// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reactive.Kql;
    using System.Reactive.Subjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using WinLog.Helpers;

    [TestClass]
    public class BooleanExpressions : TestBase
    {
        [TestMethod]
        public void AndSingle()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            TestWhere(evt, "S == \"abc\" and S == \"abc\"", true);
            TestWhere(evt, "S == \"abc\" and S != \"abc\"", false);
        }

        [TestMethod]
        public void OrArisTest()
        {
            dynamic evt = new ExpandoObject();
            evt.Source = "abc";
            TestWhere(evt, "Source == \"CertSystem\" or Source == \"CERTPublicApi\" or Source == \"CERTWebForm\"", false);
        }

        [TestMethod]
        public void AndMultiple()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            TestWhere(evt, "S == \"abc\" and S == \"abc\" and S == \"abc\"", true);
            TestWhere(evt, "S == \"abc\"  and S == \"abc\" and S != \"abc\"", false);
        }

        [TestMethod]
        public void NestedDynamic()
        {
            dynamic exp = new ExpandoObject();
            exp.x = 1;
            dynamic y = new ExpandoObject();
            y.z = 2;
            exp.y = y;                              // this results in exp = { x=1, y={ z=2}}

            TestWhere(exp, "x == 1", true);
            TestWhere(exp, "y.z == 2", true);
        }

        [TestMethod]
        public void NestedDynamicWithExtend()
        {
            dynamic exp = new ExpandoObject();
            exp.x = 1;

            dynamic y = new ExpandoObject();
            y.z = 2;
            exp.y = y;  // this results in exp = { x=1, y={ z=2}}

            var subject = new Subject<IDictionary<string, object>>();
            var result = new List<IDictionary<string, object>>();
            string query = "| extend zz = y.z | where zz == 2 | project zz";

            subject.KustoQuery(query)
                .Subscribe(e =>
                {
                    result.Add(e);
                });

            subject.OnNext(exp);

            var x = result.ToArray();

            Assert.AreEqual(x[0]["zz"], 2);
        }

        [TestMethod]
        public void NestedWhereWithIsNull()
        {
            dynamic dict = new ExpandoObject();
            dict.SubscriptionDetailsData = new
            {
                SubscriptionDetails = (ExpandoObject)null,
                SubscriptionRoomNumber = "222",
                IsSuccess = false
            };

            bool isNullResultTrue = new WhereOperator("isnull(SubscriptionDetailsData.SubscriptionDetails)").Evaluate(dict);
            Assert.AreEqual(isNullResultTrue, true);

            bool isNullResultFalse = new WhereOperator("isnull(SubscriptionDetailsData.SubscriptionRoomNumber)").Evaluate(dict);
            Assert.AreEqual(isNullResultFalse, false);

            bool isNotNullResult = new WhereOperator("not(isnotnull(SubscriptionDetailsData.SubscriptionDetails))").Evaluate(dict);
            Assert.AreEqual(isNotNullResult, true);
        }

        [TestMethod]
        public void NestedDictionaryTypeTest()
        {
            var nestedDictionary = new Dictionary<string, object>()
            {
                { "Name", "Developer" },
                { "First", "Russell" },
                { "Last", "Biles" },
                { "Age", 53 }
            };

            var exp = new Dictionary<string, object>
            {
                {"x", 1 },
                {"developer", nestedDictionary }
            };

            var subject = new Subject<IDictionary<string, object>>();
            var result = new List<IDictionary<string, object>>();
            string query = "| extend myAge = developer.Age | project myAge";

            subject.KustoQuery(query)
                .Subscribe(e =>
                {
                    result.Add(e);
                });

            subject.OnNext(exp);

            var x = result.ToArray();

            Assert.AreEqual(x[0]["myAge"], 53);
        }
    }
}