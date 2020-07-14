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
    public class SubStringTests : TestBase
    {
        [TestMethod]
        public void SubStringThreeParametersTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = substring(\"WindowsEventCollector\",0,8)");
            Assert.AreEqual(x[0].x, "WindowsE");
        }
        [TestMethod]
        public void SubStringThreeParametersFieldResolveTest()
        {
            dynamic evt = new ExpandoObject();
            evt.str = "WindowsEventCollector";
            var x = RunAtomicQuery(evt, "extend x = substring(str,0,8)");
            Assert.AreEqual(x[0].x, "WindowsE");
        }

        [TestMethod]
        public void SubStringTwoParametersTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = substring(\"WindowsEventCollector\",8)");
            Assert.AreEqual(x[0].x, "ventCollector");
        }

        [TestMethod]
        public void SubStringTwoParametersFieldResolveTest()
        {
            dynamic evt = new ExpandoObject();
            evt.str = "WindowsEventCollector";
            var x = RunAtomicQuery(evt, "extend x = substring(str,8)");
            Assert.AreEqual(x[0].x, "ventCollector");
        }

        //[TestMethod]
        //public void SubStringInvalidThirdParameterTest()
        //{
        //    dynamic evt = new ExpandoObject();

        //    try
        //    {
        //        RunAtomicQuery(evt, "extend x = substring(\"window\",6,ss)");
        //    }
        //    catch (InvalidArgumentException)
        //    {
        //        Assert.IsTrue(true);
        //    }
        //}

        [TestMethod]
        public void SubStringIndexBoundsOutside()
        {
            dynamic evt = new ExpandoObject();
            var x = RunAtomicQuery(evt, "extend x = substring(\"window\", 6, 6)");

            Assert.AreEqual(1, x.Count);
        }
    }
}