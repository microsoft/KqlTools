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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StringOperators : TestBase
    {
        // Tests of various Kusto operators described here:
        // https://kusto.azurewebsites.net/docs/queryLanguage/concepts_datatypes_string_operators.html
        [TestMethod]
        public void StringEquals()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "aBc";
            TestWhere(evt, "S == \"aBc\"", true);
            TestWhere(evt, "S == \"ABC\"", false);
            TestWhere(evt, "S == \"abc\"", false);
            TestWhere(evt, "S == \"aBcd\"", false);

            TestWhere(evt, "S =~ \"aBc\"", true);
            TestWhere(evt, "S =~ \"ABC\"", true);
            TestWhere(evt, "S =~ \"abc\"", true);
            TestWhere(evt, "S =~ \"aBcd\"", false);
        }

        [TestMethod]
        public void NestedQueryTestABorC()
        {
            dynamic evt = new ExpandoObject();
            evt.A = "5";
            evt.B = "3";
            evt.C = "2";
            //TestWhere(evt, "(B == 5 and A == 3) or C == 2", true);
            //TestWhere(evt, "(B == 5 and A == 3) or C == 1", false);
            TestWhere(evt, "C == 9 or (B == 5 and A == 3) or C == 1", false);
        }

        [TestMethod]
        public void PropertyEquals()
        {
            dynamic evt = new ExpandoObject();
            evt.A = "aBc";
            evt.B = "aBc";
            evt.C = "xxx";
            TestWhere(evt, "A == A", true);
            TestWhere(evt, "A == \"A\"", false);
            TestWhere(evt, "A == B", true);
            TestWhere(evt, "A == C", false);
        }

        [TestMethod]
        public void ToIntTests()
        {
            dynamic evt = new ExpandoObject();
            evt.A = "123";
            evt.B = 123;
            evt.C = "456";
            evt.D = 456;
            evt.E = "NaN";

            // True tests
            TestWhere(evt, "toint(A) == B", true);
            TestWhere(evt, "toint(A) == toint(B)", true);

            // False tests
            TestWhere(evt, "toint(A) == C", false);
            TestWhere(evt, "toint(A) == toint(D)", false);
            TestWhere(evt, "toint(A) == toint(E)", false);
        }

        [TestMethod]
        public void PropertyEqualsNumber()
        {
            dynamic evt = new ExpandoObject();
            evt.A = 42;
            TestWhere(evt, "A == 42", true);
        }

        [TestMethod]
        public void StringNotEquals()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "aBc";
            TestWhere(evt, "S != \"aBc\"", false);
            TestWhere(evt, "S != \"ABC\"", true);
            TestWhere(evt, "S != \"abc\"", true);
            TestWhere(evt, "S != \"aBcd\"", true);

            TestWhere(evt, "S !~ \"aBc\"", false);
            TestWhere(evt, "S !~ \"ABC\"", false);
            TestWhere(evt, "S !~ \"abc\"", false);
            TestWhere(evt, "S !~ \"aBcd\"", true);
        }

        [TestMethod]
        public void StringContains()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "FabriKam";
            TestWhere(evt, "S contains \"briK\"", true);
            TestWhere(evt, "S contains \"BRik\"", true);
            TestWhere(evt, "S contains \"BRiiii\"", false);

            TestWhere(evt, "S contains_cs \"briK\"", true);
            TestWhere(evt, "S contains_cs \"BRik\"", false);
            TestWhere(evt, "S contains_cs \"BRiiii\"", false);
        }

        [TestMethod]
        public void StringStartsWith()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "FabriKam";
            TestWhere(evt, "S startswith \"fab\"", true);
            TestWhere(evt, "S startswith \"Fab\"", true);
            TestWhere(evt, "S startswith \"BRiiii\"", false);

            TestWhere(evt, "S startswith_cs \"fab\"", false);
            TestWhere(evt, "S !startswith_cs \"fab\"", true);
            TestWhere(evt, "S startswith \"Fab\"", true);
            TestWhere(evt, "S startswith \"BRiiii\"", false);
        }

        [TestMethod]
        public void StringEndsWith()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "FabriKam";
            TestWhere(evt, "S endswith \"kam\"", true);
            TestWhere(evt, "S endswith \"Kam\"", true);
            TestWhere(evt, "S endswith \"BRiiii\"", false);

            TestWhere(evt, "S endswith_cs \"kam\"", false);
            TestWhere(evt, "S !endswith_cs \"kam\"", true);
            TestWhere(evt, "S endswith \"Kam\"", true);
            TestWhere(evt, "S endswith \"BRiiii\"", false);
        }

        [TestMethod]
        public void StringIn()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            evt.N = 123;
            TestWhere(evt, "S in (\"123\", \"abc\")", true);
            TestWhere(evt, "S in (\"123\", \"ABC\")", false);
            TestWhere(evt, "N in (123, 456)", true);
            TestWhere(evt, "N in (456, \"789\")", false);
        }

        [TestMethod]
        public void StringNotIn()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            evt.N = 123;
            TestWhere(evt, "S !in (\"123\", \"abc\")", false);
            TestWhere(evt, "S !in (\"123\", \"ABC\")", true);
            TestWhere(evt, "N !in (123, 456)", false);
            TestWhere(evt, "N !in (456, \"789\")", true);
        }

        [TestMethod]
        public void StringIn_CaseInsentitive()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            evt.N = 123;
            TestWhere(evt, "S in~ (\"123\", \"ABC\")", true);
        }

        [TestMethod]
        public void StringNotIn_CaseInsentitive()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            evt.N = 123;
            TestWhere(evt, "S !in~ (\"123\", \"ABC\")", false);
        }

        [TestMethod]
        public void IsNull()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            TestWhere(evt, "isnull(S)", false);
            TestWhere(evt, "isnull(N)", true);
        }

        [TestMethod]
        public void IsEmpty()
        {
            dynamic evt = new ExpandoObject();
            evt.ThreeChars = "abc";
            evt.EmptyStr = string.Empty;
            evt.Null = null;

            TestWhere(evt, "isempty(ThreeChars)", false);
            TestWhere(evt, "isempty(EmptyStr)", true);
            TestWhere(evt, "isempty(Null)", true);
        }

        [TestMethod]
        public void IsNotNull()
        {
            dynamic evt = new ExpandoObject();
            evt.S = "abc";
            TestWhere(evt, "isnotnull(S)", true);
            TestWhere(evt, "isnotnull(N)", false);
        }

        [TestMethod]
        public void StringReverseTests()
        {
            dynamic evt = new ExpandoObject();

            evt.S = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            TestWhere(evt, "reverse(S) == \"ZYXWVUTSRQPONMLKJIHGFEDCBA\"", true);

            evt.int12345 = 12345;
            TestWhere(evt, "reverse(int12345) == 54321", true);

            evt.double12345 = 123.45;
            TestWhere(evt, "reverse(double12345) == 54.321", true);
        }

        [TestMethod]
        public void TrimStartTests()
        {
            dynamic evt = new ExpandoObject();

            evt.S = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            // Remove the first part test
            TestWhere(evt, "trim_start(\"ABC\", S) == \"DEFGHIJKLMNOPQRSTUVWXYZ\"", true);
            TestWhere(evt, "trim_start(\"A.*G\", S) == \"HIJKLMNOPQRSTUVWXYZ\"", true);

            // Remove nothing, compare to the original string
            TestWhere(evt, "trim_start(\"1.*8\", S) == \"ABCDEFGHIJKLMNOPQRSTUVWXYZ\"", true);
        }

        [TestMethod]
        public void TrimEndTests()
        {
            dynamic evt = new ExpandoObject();

            evt.S = "ABC_XYZ_ABC_XYZ";

            TestWhere(evt, "trim_end(\"XYZ\", S) == \"ABC_XYZ_ABC_\"", true);
            TestWhere(evt, "trim_end(\"X.Z\", S) == \"ABC_XYZ_ABC_\"", true);

            // No RegEx match test
            TestWhere(evt, "trim_end(\"123\", S) == \"ABC_XYZ_ABC_XYZ\"", true);

            // Case sensitive test, based on the wrong case in the RegEx pattern
            TestWhere(evt, "trim_end(\"abc\", S) == \"ABC_XYZ_ABC_\"", false);
        }

        [TestMethod]
        public void TrimTests()
        {
            dynamic evt = new ExpandoObject();

            evt.S = "ABC_XYZ_ABC_XYZ_ABC";

            TestWhere(evt, "trim(\"ABC\", S) == \"_XYZ_ABC_XYZ_\"", true);

            // No RegEx match test
            TestWhere(evt, "trim(\"123\", S) == \"ABC_XYZ_ABC_XYZ_ABC\"", true);
        }

        [TestMethod]
        public void IndexOfStringTests()
        {
            dynamic evt = new ExpandoObject();

            string query1 = "extend idx1 = indexof(\"abcdefg\",\"cde\")"; // lookup found in input string
            var result1 = RunAtomicQuery(evt, query1);
            Assert.AreEqual(result1[0].idx1, 2);

            string query2 = "extend idx2 = indexof(\"abcdefg\",\"cde\",1,4)"; // lookup found in researched range 
            var result2 = RunAtomicQuery(evt, query2);
            Assert.AreEqual(result2[0].idx2, 2);

            string query3 = "extend idx3 = indexof(\"abcdefg\",\"cde\",1,2)"; // search starts from index 1, but stops after 2 chars, so full lookup can't be found
            var result3 = RunAtomicQuery(evt, query3);
            Assert.AreEqual(result3[0].idx3, -1);

            string query4 = "extend idx4 = indexof(\"abcdefg\",\"cde\",3,4)"; // search starts after occurrence of lookup
            var result4 = RunAtomicQuery(evt, query4);
            Assert.AreEqual(result4[0].idx4, -1);

            // string query5 = "extend idx5 = indexof(\"abcdefg\",\"cde\", -1)"; // invalid input
            // TODO: Resolve negative parameter parsing.  
            // var result5 = RunAtomicQuery(evt, query5);
            // Assert.AreEqual(result5[0].idx5, 2);

            string query6 = "extend idx6 = indexof(1234567,5,1,4)"; // two first parameters were forcibly casted to strings "12345" and "5"
            var result6 = RunAtomicQuery(evt, query6);
            Assert.AreEqual(result6[0].idx6, 4);

            // string query7 = "extend idx7 = indexof(\"abcdefg\",\"cde\",2, -1)"; // lookup found in input string
            // TODO: Resolve negative parameter parsing.  
            // var result7 = RunAtomicQuery(evt, query7);
            // Assert.AreEqual(result7[0].idx7, 2);

            string query8 = "extend idx8 = indexof(\"abcdefgabcdefg\", \"cde\", 1, 10, 2)"; // lookup found in input range
            var result8 = RunAtomicQuery(evt, query8);
            Assert.AreEqual(result8[0].idx8, 9);

            // string query9 = "extend idx9 = indexof(\"abcdefgabcdefg\", \"cde\", 1, -1, 3)"; // the third occurrence of lookup is not in researched range
            // TODO: Resolve negative parameter parsing.  
            // var result9 = RunAtomicQuery(evt, query9);
            // Assert.AreEqual(result9[0].idx9, 2);
        }

        [TestMethod]
        public void IndexOfRegExTests()
        {
            dynamic evt = new ExpandoObject();

            string query1 = "extend idx1 = indexof_regex(\"abcabc\", \"a.c\")"; // lookup found in input string
            var result1 = RunAtomicQuery(evt, query1);
            Assert.AreEqual(result1[0].idx1, 0);

            string query2 = "extend idx2 = indexof_regex(\"abcabcdefg\", \"a.c\", 0, 9, 2)"; // lookup found in input string 
            var result2 = RunAtomicQuery(evt, query2);
            Assert.AreEqual(result2[0].idx2, 3);

            string query3 = "extend idx3 = indexof_regex(\"abcabcdefg\", \"b.d\", 0, 9, 1)"; // lookup found in input string 
            var result3 = RunAtomicQuery(evt, query3);
            Assert.AreEqual(result3[0].idx3, 4);

            string query4 = "extend idx4 = indexof_regex(\"abcabcdefg\", \"x.z\", 0, 9, 1)"; // lookup cannot be found in input string 
            var result4 = RunAtomicQuery(evt, query4);
            Assert.AreEqual(result4[0].idx4, -1);

            // string query6 = "extend idx3 = indexof_regex(\"abcabc\", \"a.c\", 1, -1, 2)"; // there is no second occurrence in the search range
            // TODO: Resolve negative parameter parsing.  
            // var result3 = RunAtomicQuery(evt, query3);
            // Assert.AreEqual(result3[0].idx3, -1);

            // string query7 = "extend idx4 = indexof_regex(\"ababaa\", \"a.a\", 0, -1, 2)"; // Plain string matches do not overlap so full lookup can't be found
            // TODO: Resolve negative parameter parsing.  
            // var result4 = RunAtomicQuery(evt, query4);
            // Assert.AreEqual(result4[0].idx4, -1);

            // string query5 = "extend idx5 = indexof_regex(\"abcabc\", \"a|ab\", -1)"; // invalid input
            // TODO: Resolve negative parameter parsing.  
            // var result5 = RunAtomicQuery(evt, query5);
            // Assert.AreEqual(result5[0].idx5, 2);
        }

        [TestMethod]
        public void CoalesceTests()
        {
            // result=coalesce(tolong("not a number"), tolong("42"), 33)
            dynamic evt = new ExpandoObject();
            evt.NullValue = null;
            evt.IntValue = 12345;

            DateTime testUtcDateTime = DateTime.UtcNow;
            evt.Now = testUtcDateTime;

            string query1 = "extend result=coalesce(tolong(\"not a number\"), tolong(\"42\"), 33)"; // Long 42 should be returned
            var result1 = RunAtomicQuery(evt, query1);
            Assert.AreEqual(result1[0].result, 42);

            string query2 = "extend result=coalesce(UnknownField, NullValue, 75)"; // 75 should be returned, after a missing field and a null field
            var result2 = RunAtomicQuery(evt, query2);
            Assert.AreEqual(result2[0].result, 75);

            string query3 = "extend result=coalesce(NullValue, IntValue)"; // 12345 should be returned, after a missing field and a null field
            var result3 = RunAtomicQuery(evt, query3);
            Assert.AreEqual(result3[0].result, 12345);

            string query4 = "extend result=coalesce(NullValue, Now)"; // UTC at execution time should be returned, after a missing field and a null field
            var result4 = RunAtomicQuery(evt, query4);
            Assert.AreEqual(result4[0].result, testUtcDateTime);
        }

        [TestMethod]
        public void RandomIsNotNullTests()
        {
            dynamic dict = new ExpandoObject();
            dict.A = new
            {
                B = "panguye@ame.gbl"
            };

            bool evaluationResult = new WhereOperator("isnull(A.B)").Evaluate(dict);
            Assert.AreEqual(evaluationResult, false);

            evaluationResult = new WhereOperator("isnotnull(A.B)").Evaluate(dict);
            Assert.AreEqual(evaluationResult, true);

            evaluationResult = new WhereOperator("not(isnotnull(A.B))").Evaluate(dict);
            Assert.AreEqual(evaluationResult, false);
        }

        [TestMethod]
        public void EscapedDoubleQuoteStringTest_Simple()
        {
            dynamic dict = new ExpandoObject();

            string extendStatement = @"extend str = strcat(""this is a "", ""\""string\"""")";
            var extendOperator = new ExtendOperator(extendStatement);
            dynamic e = extendOperator.Extend(dict);

            Assert.AreEqual(e.str, "this is a \"string\"");
        }

        [TestMethod]
        public void EscapedDoubleQuoteStringTest_Xml()
        {
            dynamic dict = new ExpandoObject();
            // var dkj = @"extend str = strcat(""<html xmlns: v =\""urn:schemas-microsoft-com:vml\""-xmlns"", "":o=\""urn:schemas-microsoft-com:office:office\""-xmlns:w=\""urn:schemas-microsoft-com:office:word\"""")";
            var strcatHtml =
                @"extend str = strcat(""<html xmlns: v =\""urn:schemas-microsoft-com:vml\"" xmlns"", "":o=\""urn:schemas-microsoft-com:office:office\"" xmlns:w=\""urn:schemas-microsoft-com:office:word\"""")";
            Console.WriteLine(strcatHtml);
            var extendOperator = new ExtendOperator(strcatHtml);
            dynamic e = extendOperator.Extend(dict);
            Console.WriteLine(e.str);
        }

        [TestMethod]
        public void StrCatExtremeStringTest_Xml()
        {
            dynamic dict = new ExpandoObject();
            string strcatHtml =
                "extend str = strcat(\"ObjectProperty\", \" <br><strong>ArisReportSource: </strong> \", \"original.Source\", \" <br><strong>DetectionId: </strong> \", \"original.Data.DetectionId\", \" <br><strong>Data.ACSVersion: </strong> \", \"original.Data.Data.ACSVersion\", \" <br><strong>Data.Action: </strong> \", \"original.Data.Data.Action\", \" <br><strong>Data.Category: </strong> \", \"original.Data.Data.Category\", \" <br><strong>UserName: </strong> \", \"original.Data.UserName\", \" <br><strong>sourceAddress: </strong> \", \"original.Data.sourceAddress\", \" <br><strong>destinationAddress: </strong> \", \"original.Data.destinationAddress\", \" <br><strong>Data.HostName: </strong> \", \"original.Data.Data.HostName\", \" <br><strong>Data.Device IP Address: </strong> \", \"original.Data.Data.[\'Device_IP_Address\']\", \" <br><strong>Data.Remote-Address: </strong> \", \"original.Data.Data.[\'Remote-Address\']\", \" <br><strong>Data.SelectedShellProfile: </strong> \", \"original.Data.Data.SelectedShellProfile\", \" <br><strong>Data.UserName: </strong> \", \"original.Data.Data.UserName\", \" <br><strong>Data.AD-Groups-Names: </strong> \", \"original.Data.Data.[\'AD-Groups-Names\']\", \" <br><strong>Data.AD-User-Candidate-Identities: </strong> \", \"original.Data.[\'Data.AD-User-Candidate-Identities\']\", \" <br><strong>Data.NetworkDeviceGroups[0]: </strong> \", \"original.Data.Data.NetworkDeviceGroups[0]\", \" <br><strong>Data.NetworkDeviceName: </strong> \", \"original.Data.Data.NetworkDeviceName\", \" <br><strong>Message: </strong> \", \"original.Data.Message\", \" <br><strong>IngestionTime: </strong> \", \"original.Data.IngestionTime\", \" <br><strong>ReceivedDateTime: </strong> \", \"original.Data.ReceivedDateTime\", \" <br>\")";

            Console.WriteLine(strcatHtml);
            var extendOperator = new ExtendOperator(strcatHtml);
            dynamic e = extendOperator.Extend(dict);
            Console.WriteLine(e.str);
        }

        [TestMethod]
        public void StrCatStringFieldTest_Xml()
        {
            dynamic root = new ExpandoObject();

            dynamic dict = new Dictionary<string, object>();
            dict.Add("Field 1", "The first field");
            dict.Add("Field 2", "The second field");

            root.Fields = dict;

            string strcatHtml =
                "extend str = strcat(Fields[\"Field 1\"], \" <br><strong>ArisReportSource: </strong> \", Fields[\"Field 2\"], \" <br><strong>ArisReportSource: </strong> \")";

            Console.WriteLine(strcatHtml);
            var e = new ExtendOperator(strcatHtml).Extend(root);
            Console.WriteLine(e.str);
        }

        [TestMethod]
        public void StrCatStringNonExistentFieldTest_Xml()
        {
            dynamic root = new ExpandoObject();

            dynamic dict = new Dictionary<string, object>();
            dict.Add("Field 1", "The first field");
            dict.Add("Field 2", "The second field");

            root.Fields = dict;

            string strcatHtml =
                "extend str = strcat(Fields[\"Field 1\"], \" <br><strong>ArisReportSource: </strong> \", Fields[\"Field 3\"], \" <br><strong>ArisReportSource: </strong> \")";

            var e = new ExtendOperator(strcatHtml).Extend(root);
            Assert.AreEqual(e.str, "The first field <br><strong>ArisReportSource: </strong>  <br><strong>ArisReportSource: </strong> ");
        }
    }
}