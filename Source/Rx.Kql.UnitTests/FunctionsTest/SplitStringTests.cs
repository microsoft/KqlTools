// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System.Collections.Generic;
using System.Reactive.Kql;
using System.Reactive.Subjects;

namespace Rx.Kql.UnitTest.FunctionsTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Dynamic;

    [TestClass]
    public class SplitStringTests : TestBase
    {
        [TestMethod]
        public void SimpleSplitStringTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = split(\"aa_bb\",\"_\")");
            Assert.AreEqual(x[0].x[0], "aa");
            Assert.AreEqual(x[0].x[1], "bb");
        }

        [TestMethod]
        public void SplitStringWithRequestedIndexTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = split(\"aaa_bbb_ccc\",\"_\", 1)");
            Assert.AreEqual(x[0].x, "bbb");
        }

        [TestMethod]
        public void SplitStringWithEmptyInputTest()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = split(\"\",\"_\")");
            Assert.AreEqual(x[0].x, "");
        }

        [TestMethod]
        public void SplitTestWithEmptyStringReturn()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = split(\"a__b\",\"_\")");
            Assert.AreEqual(x[0].x[0], "a");
            Assert.AreEqual(x[0].x[1], "");
            Assert.AreEqual(x[0].x[2], "b");
        }

        [TestMethod]
        public void SplitStringWithMultiCharSplitter()
        {
            dynamic evt = new ExpandoObject();

            var x = RunAtomicQuery(evt, "extend x = split(\"aabbcc\", \"bb\")");
            Assert.AreEqual(x[0].x[0], "aa");
            Assert.AreEqual(x[0].x[1], "cc");
        }

        [TestMethod]
        public void SplitTestWithReplaceExtend()
        {
            dynamic evt = new ExpandoObject();

            evt.stringtoreplace = "::ffff:";
            evt.replacestring = "";
            evt.inputstring = "::ffff:10.83.80.127:54544";
            evt.splitstring = ":";
            var z = RunAtomicQueryWithKqlNode(evt, "TableName | extend x =  split(replace(stringtoreplace,replacestring,inputstring), splitstring)");

            Assert.AreEqual(z[0].x[0], "10.83.80.127");
            Assert.AreEqual(z[0].x[1], "54544");
        }

        [TestMethod]
        public void SplitTestWithReplaceProject()
        {
            dynamic evt = new ExpandoObject();

            evt.stringtoreplace = "::ffff:";
            evt.replacestring = "";
            evt.inputstring = "::ffff:10.83.80.127:54544";
            evt.splitstring = ":";

            string query = "extend x =  split(replace(stringtoreplace,replacestring,inputstring), splitstring) | extend ip = x[0], port = x[1] | project ip, port";
            var z = RunAtomicQuery(evt, query);

            Assert.AreEqual(z[0].ip, "10.83.80.127");
            Assert.AreEqual(z[0].port, "54544");

            query = "Table | extend x =  split(replace(stringtoreplace,replacestring,inputstring), splitstring) | extend ip = x[0], port = x[1] | project ip, port";
            var zz = RunAtomicQueryWithKqlNode(evt, query);
            var yy = zz.ToArray();
            var aa = (IDictionary<string, object>) yy[0];

            Assert.AreEqual(aa["ip"], "10.83.80.127");
            Assert.AreEqual(aa["port"], "54544");
        }
    }
}