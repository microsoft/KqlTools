// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests.FunctionsTest
{
    using System.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CaseTests : TestBase
    {
        [TestMethod]
        public void CaseThreeOptionsThenValueIsConstantTest()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, \"Small\", Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "Small");

            evt = new ExpandoObject();
            evt.Size = 5;

            x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, \"Small\", Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "Medium");

            evt = new ExpandoObject();
            evt.Size = 100;

            x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, \"Small\", Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "Large");
        }

        [TestMethod]
        public void CaseExpressionBeingBooleanValue()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(true, \"Small\", Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "Small");
        }

        [TestMethod]
        public void CaseExpressionBeingScalarFunctionTest()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(tobool(Size <= 3), \"Small\" , Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "Small");
        }

        [TestMethod]
        public void CaseThenValueIsScalarFunctionTest()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, strcat(\"this is a \", Size) , Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "this is a 2");
        }

        [TestMethod]
        public void CaseThenValueIsExpressionTest()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, strcat(\"String1 \", strcat(\"this is a \", Size)) , Size <= 10, \"Medium\", \"Large\")");
            Assert.AreEqual(x[0].bucket, "String1 this is a 2");
        }

        [TestMethod]
        public void CaseScalarValueTest()
        {
            dynamic evt = new ExpandoObject();
            evt.NoMatchEventId = 46978;
            evt.EventId = 4697;
            evt.ServiceFileName = "FileName-01";
            evt.ImagePath = "ImagePath-01";

            var x = RunAtomicQuery(evt, "extend ServicePath = case(NoMatchEventId == \'4697\', toupper(tostring(ServiceFileName)),NoMatchEventId == \'7045\', toupper(tostring(ImagePath)), \"none\")");
            Assert.AreEqual(x[0].ServicePath, "none");

            var y = RunAtomicQuery(evt, "extend ServicePath = case(EventId == \'4697\', toupper(tostring(ServiceFileName)),EventId == \'7045\', toupper(tostring(ImagePath)), \"none\")");
            Assert.AreEqual(y[0].ServicePath, "FILENAME-01");
        }

        /*
        [TestMethod]
        [ExpectedException(typeof(InvalidArgumentException), "Case Function: Invalid Comparision Expression")]
        public void CaseExpressionBeingScalarProperty()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size, \"Small\", Size <= 10, \"Medium\", \"Large\"");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidArgumentException), "Case Function: Scalar Constant or Scalar Function Expected")]
        public void CaseExpressionWithOneParam()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3)");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidArgumentException), "Case Function: No Else part present")]
        public void CaseExpressionWithNoParams()
        {
            dynamic evt = new ExpandoObject();
            var x = RunAtomicQuery(evt, "extend bucket = case()");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidArgumentException), "Case Function: No Else part present")]
        public void InvalidCaseStatementWithNoElse()
        {
            dynamic evt = new ExpandoObject();
            evt.Size = 2;

            var x = RunAtomicQuery(evt, "extend bucket = case(Size <= 3, \"Small\", Size <= 10, \"Medium\"");
        }
        */
    }
}