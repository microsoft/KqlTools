// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTest
{
    using System;
    using System.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NumericOperators : TestBase
    {
        [TestMethod]
        public void GreaterThanLessThanLongDataTypes()
        {
            dynamic evt = new ExpandoObject();
            evt.Num = 5L;
            evt.LongNum = (long)Math.Pow(1024, 4);
            TestWhere(evt, "Num > 3", true);
            TestWhere(evt, "LongNum >= 3", true);
            TestWhere(evt, "LongNum <= 3", false);
        }

        [TestMethod]
        public void GreaterThanLessThanIntDataTypes()
        {
            dynamic evt = new ExpandoObject();
            evt.Num = 5;
            evt.IntNum = (int)Math.Pow(1024, 2);
            TestWhere(evt, "Num > 3", true);
            TestWhere(evt, "IntNum >= 3", true);
            TestWhere(evt, "IntNum <= 3", false);
        }

        [TestMethod]
        public void Between()
        {
            dynamic evt = new ExpandoObject();
            evt.target = 34;
            evt.min = 30;
            evt.max = 37;
            TestWhere(evt, "Between(target,min,max)", true);
            TestWhere(evt, "Between(target,14,16)", false);
            TestWhere(evt, "Between(target,10,1000)", true);

            TestWhere(evt, "target between(min..max)", true); //preferred, official Kusto syntax
            TestWhere(evt, "target between(14..16)", false);
            TestWhere(evt, "target between(10..1000)", true);
        }

        [TestMethod]
        public void NotBetween()
        {
            dynamic evt = new ExpandoObject();
            evt.target = 34;
            evt.min = 30;
            evt.max = 37;
            TestWhere(evt, "NotBetween(target,min,max)", false); //BREAKING CHANGE from Rx.Kql: using Not instead of !
            TestWhere(evt, "NotBetween(target,14,16)", true); //though you should use the target between (min..max) syntax to begin with
            TestWhere(evt, "NotBetween(target,10,1000)", false);

            TestWhere(evt, "target !between(min..max)", false); //preferred, official Kusto syntax
            TestWhere(evt, "target !between(14..16)", true); 
            TestWhere(evt, "target !between(10..1000)", false);
        }
    }
}