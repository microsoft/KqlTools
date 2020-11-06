// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests.FunctionsTest
{
    using System;
    using System.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReplaceTests : TestBase
    {
        [TestMethod]
        public void ReplaceValidTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = replace(\"e\",\"X\",\"Weventcellecter\")");
            Assert.AreEqual(x[0].x, "WXvXntcXllXctXr");
        }
        [TestMethod]
        public void ReplaceValidResolveFieldTest()
        {
            dynamic evt = new ExpandoObject();
            evt.stringtoreplace = "e";
            evt.replacestring = "X";
            evt.inputstring = "Weventcellecter";
            var x = RunAtomicQuery(evt, "extend x = replace(stringtoreplace,replacestring,inputstring)");
            Assert.AreEqual(x[0].x, "WXvXntcXllXctXr");
        }
        [TestMethod]
        public void ReplaceMissingParametersTest()
        {
            dynamic evt = new ExpandoObject();

            try
            {
                var x = RunAtomicQuery(evt, "extend x = replace(\"e\",\"Weventcellecter\")");
            }
            catch (Exception)
            {
                Assert.IsTrue(true);
            }
        }
    }
}