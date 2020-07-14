// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.UnitTest.FunctionsTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Dynamic;

    [TestClass]
    public class ExtractTests : TestBase
    {
        [TestMethod]
        public void ExtractUsingRegExTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = extract(\"x=([0-9.]+)\",1,\"hello x=45.6 | wo\")");
            Assert.AreEqual(x[0].x, "45.6");
        }

        [TestMethod]
        public void ExtractUsingRegExWithTypeInputTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = extract(\"Duration=([0-9.]+)\",1,\"A=1, B=2, Duration=123.45\", typeof(real))");
            Assert.AreEqual(x[0].x, 123.45);
        }
    }
}