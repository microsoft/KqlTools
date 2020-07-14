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
    public class FormatDateTimeTests: TestBase
    {
        [TestMethod]
        public void FormatDateTimeTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = format_datetime(datetime(2015-12-14 02:03:04.12345), 'y-M-d h:m:s.fffffff')");
            Assert.AreEqual(x[0].x, "15-12-14 2:3:4.1234500");
        }

        [TestMethod]
        public void FormatDateTimeTest1()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = format_datetime(datetime(2017-01-29 09:00:05), 'yy-MM-dd [HH:mm:ss]')");
            Assert.AreEqual(x[0].x, "17-01-29 [09:00:05]");
        }

        [TestMethod]
        public void FormatDateTimeTest2()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = format_datetime(datetime(2017-01-29 09:00:05), 'yyyy-M-dd [H:mm:ss]')");
            Assert.AreEqual(x[0].x, "2017-1-29 [9:00:05]");
        }

        [TestMethod]
        public void FormatDateTimeTest3()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = format_datetime(datetime(2017-01-29 09:00:05), 'yy-MM-dd [hh:mm:ss tt]')");
            Assert.AreEqual(x[0].x, "17-01-29 [09:00:05 AM]");
        }
    }
}
