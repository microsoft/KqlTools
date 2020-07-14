// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.UnitTest.FunctionsTest
{
    using System;
    using System.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class IsNotEmptyTests : TestBase
    {
        [TestMethod]
        public void IsNotEmptyFalseTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = isnotempty()");
          
            Assert.IsFalse(x[0].x);
        }

        [TestMethod]
        public void IsNotEmptyTrueTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = isnotempty(\"aBc\")");
            Assert.IsTrue(x[0].x);
        }

        [TestMethod]
        public void IsNotEmptyFalseFieldResolveTest()
        {
            dynamic evt = new ExpandoObject();
            evt.str = string.Empty;
            var x = RunAtomicQuery(evt, "extend x = isnotempty(str)");

            Assert.IsFalse(x[0].x);
        }

        [TestMethod]
        public void IsNotEmptyTrueFieldResolveTest()
        {
            dynamic evt = new ExpandoObject();
            evt.sdf = "NonEmptyString";

            var x = RunAtomicQuery(evt, "extend x = isnotempty(sdf)");
            Assert.IsTrue(x[0].x);
        }
    }
}