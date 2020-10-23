// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reactive.Kql;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExtendTests : TestBase
    {
        [TestMethod]
        public void ExtendScalarConstAndPropSerializationDoubleQuoteTest()
        {
            string originalString = "extend text = AFieldName, intFive = 5, stringThree = \"Three\"";

            ExtendOperator extendOperator = new ExtendOperator(originalString);

            string serializationOfExtend = RxKqlCommonFunctions.ToJson(extendOperator);

            ExtendOperator hydratedExtendOperator = RxKqlCommonFunctions.ToExtendOperator(serializationOfExtend);

            Assert.AreEqual(originalString, hydratedExtendOperator.ToString());
        }

        [TestMethod]
        public void ExtendReplaceFunctionSerializationTest()
        {
            string originalString =
                "extend result = replace(\"@ame.gbl\", \"@microsoft.com\", SubscriptionDetailsData.AccountAdminLiveEmailId)";

            ExtendOperator extendOperator = new ExtendOperator(originalString);

            string serializationOfExtend = RxKqlCommonFunctions.ToJson(extendOperator);

            ExtendOperator hydratedExtendOperator = RxKqlCommonFunctions.ToExtendOperator(serializationOfExtend);

            Assert.AreEqual(originalString, hydratedExtendOperator.ToString());
        }

        [TestMethod]
        public void ExtendSimpleTest()
        {
            var extend = new ExtendOperator("name = \"text\"");
            IDictionary<string, object> extended = extend.Extend(new Dictionary<string, object>());
            var result = extended["name"].ToString();
            Assert.AreEqual(result, "text");
        }

        [TestMethod]
        public void ExtendScalarFunctionSerializationTest()
        {
            List<string> listExtendTestScalarFunctions = new List<string>()
            {
                "extend text = strcat(\"Case \", original.id)",
                "extend text = strlen(\"1234567890\")",
                "extend text = tostring(123)"
            };

            foreach (string extendedScalarFunction in listExtendTestScalarFunctions)
            {
                ExtendOperator extendOperator = new ExtendOperator(extendedScalarFunction);

                string serializationOfExtend = RxKqlCommonFunctions.ToJson(extendOperator);

                ExtendOperator hydratedExtendOperator = RxKqlCommonFunctions.ToExtendOperator(serializationOfExtend);

                Assert.AreEqual(extendedScalarFunction, hydratedExtendOperator.ToString());
            }
        }

        [TestMethod]
        public void ExtendOne()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "hello";
            var x = RunAtomicQuery(evt, "extend E = H");
            Assert.AreEqual(x[0].H, "hello");
            Assert.AreEqual(x[0].E, "hello");
        }

        [TestMethod]
        public void ExtendConst()
        {
            dynamic evt = new ExpandoObject();
            var x = RunAtomicQuery(evt, "extend E = \"hello\"");
            Assert.AreEqual(x[0].E, "hello");
            var y = RunAtomicQuery(evt, "extend E = 3");
            Assert.AreEqual(y[0].E, 3);
        }

        [TestMethod]
        public void ExtendStrcat()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "hello";
            evt.W = "world";
            var x = RunAtomicQuery(evt, "extend x = strcat(H,\" \",W)");
            Assert.AreEqual(x[0].x, "hello world");
        }

        [TestMethod]
        public void ToLower()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "HELLO";
            evt.W = "WORLD";
            var x = RunAtomicQuery(evt, "extend x = tolower(H), y = tolower(W), z = tolower(\"CONSTANTSTR\")");
            Assert.AreNotEqual(x[0].x, "HELLO");
            Assert.AreEqual(x[0].y, "world");
            Assert.AreEqual(x[0].z, "constantstr");
        }

        [TestMethod]
        public void ToUpper()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "hello";
            evt.W = "world";
            var x = RunAtomicQuery(evt, "extend x = toupper(H), y = toupper(W), z = toupper(\"constantstr\")");
            Assert.AreEqual(x[0].x, "HELLO");
            Assert.AreNotEqual(x[0].y, "world");
            Assert.AreEqual(x[0].z, "CONSTANTSTR");
        }

        [TestMethod]
        public void Bag_Unpack_Simple()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "hello";
            evt.W = "world";

            dynamic packed = new ExpandoObject();
            packed.H1 = "I'm";
            packed.W1 = "alive";
            packed.N1 = 123;

            evt.pack = packed;

            var x = RunAtomicQuery(evt, "evaluate bag_unpack(pack)");

            Assert.AreEqual(packed.H1, x[0].H1);
            Assert.AreEqual(packed.N1, x[0].N1);
        }

        [TestMethod]
        public void Bag_Unpack_PrefixUnpackedFields()
        {
            dynamic evt = new ExpandoObject();
            evt.H = "hello";
            evt.W = "world";

            dynamic packed = new ExpandoObject();
            packed.H1 = "I'm";
            packed.W1 = "alive";
            packed.N1 = 123;

            evt.pack = packed;

            var x = RunAtomicQuery(evt, "evaluate bag_unpack(pack, \"prefix_\")");

            Assert.AreEqual(packed.H1, x[0].prefix_H1);
            Assert.AreEqual(packed.N1, x[0].prefix_N1);
        }

        [TestMethod]
        public void ExtendStrlen()
        {
            dynamic evt = new ExpandoObject();
            evt.UnEqualCommandLine = "let's start a crazy process, and break the internet!";
            evt.EqualCommandLine = "this one should be equivalent!";

            // Negative test...
            var x = RunAtomicQuery(evt, "extend x = strlen(UnEqualCommandLine)");
            Assert.AreNotEqual(Convert.ToInt32(x[0].x.ToString()), 51); // Should be 52

            // Positive test...
            var y = RunAtomicQuery(evt, "extend y = strlen(EqualCommandLine)");
            Assert.AreEqual(Convert.ToInt32(y[0].y.ToString()), 30);
        }

        [TestMethod]
        public void Ipv4FromNumber()
        {
            dynamic evt = new ExpandoObject();
            evt.UnEqualCommandLine = "let's start a crazy process, and break the internet!";
            evt.EqualCommandLine = "this one should be equivalent!";

            // Negative test...
            var x = RunAtomicQuery(evt, "extend x = strlen(UnEqualCommandLine)");
            Assert.AreNotEqual(Convert.ToInt32(x[0].x.ToString()), 51); // Should be 52

            // Positive test...
            var y = RunAtomicQuery(evt, "extend y = strlen(EqualCommandLine)");
            Assert.AreEqual(Convert.ToInt32(y[0].y.ToString()), 30);
        }

        [TestMethod]
        public void Parse_Ipv4()
        {
            dynamic evt = new ExpandoObject();
            evt.ipv4 = "127.0.0.1"; // 2130706433
            evt.ipv4_1 = "192.1.168.1"; // 3221334017
            evt.ipv4_2 = "192.1.168.2"; // 3221334018

            // Negative test...
            var x = RunAtomicQuery(evt, "extend x = parse_ipv4(\"192.1.168.2\")");
            Assert.AreNotEqual(Convert.ToInt64(x[0].x.ToString()), 2130706433);

            // Positive test...
            var y = RunAtomicQuery(evt, "extend y = parse_ipv4(\"127.0.0.1\")");
            Assert.AreEqual(Convert.ToInt64(y[0].y.ToString()), 2130706433);
        }

        /// <summary>
        /// IIF Tests
        /// </summary>
        [TestMethod]
        public void ImmediateIfTests()
        {
            dynamic evt = new ExpandoObject();
            evt.Timestamp = DateTime.Now;
            evt.target = 34;
            evt.min = 30;
            evt.max = 37;
            evt.nestedValue = 35;
            evt.outsidetarget = 39;

            // IIF Tests
            // Negative test...
            var resultFalse = RunAtomicQuery(evt, "extend comparison = iif(target > 35, \"greater\", \"less than\")");
            var y = resultFalse.ToArray();
            Assert.AreEqual(y[0].comparison.ToString(), "less than");

            // Postitive test...
            var resultTrue = RunAtomicQuery(evt, "extend comparison = iif(target between(min..max), \"between\", \"not between\")");
            var x = resultTrue.ToArray();
            Assert.AreEqual(x[0].comparison.ToString(), "between");

            // Postitive test...
            var resultNested = RunAtomicQuery(evt, "extend comparison = iif(target between(min..max), iif(nestedValue == 35, \"equal\", \"not equal\"), \"not between\")");
            var z = resultNested.ToArray();
            Assert.AreEqual(z[0].comparison.ToString(), "equal");

            // Negative test...
            var resultNestedNegative = RunAtomicQuery(evt, "extend comparison = iif(outsidetarget between(min..max), iif(nestedValue == 35, \"equal\", \"not equal\"), \"not between\")");
            var a = resultNestedNegative.ToArray();
            Assert.AreEqual(a[0].comparison.ToString(), "not between");
        }

        /// <summary>
        /// IFF Tests
        /// </summary>
        [TestMethod]
        public void ImmediateFfTests()
        {
            dynamic evt = new ExpandoObject();
            evt.Timestamp = DateTime.Now;
            evt.target = 34;
            evt.min = 30;
            evt.max = 37;
            evt.nestedValue = 35;
            evt.outsidetarget = 39;

            // IIF Tests
            // Negative test...
            var resultFalse = RunAtomicQuery(evt, "extend comparison = iff(target > 35, \"greater\", \"less than\")");
            var y = resultFalse.ToArray();
            Assert.AreEqual(y[0].comparison.ToString(), "less than");

            // Postitive test...
            var resultTrue = RunAtomicQuery(evt, "extend comparison = iff(target between(min..max), \"between\", \"not between\")");
            var x = resultTrue.ToArray();
            Assert.AreEqual(x[0].comparison.ToString(), "between");

            // Postitive test...
            var resultNested = RunAtomicQuery(evt, "extend comparison = iff(target between(min..max), iff(nestedValue == 35, \"equal\", \"not equal\"), \"not between\")");
            var z = resultNested.ToArray();
            Assert.AreEqual(z[0].comparison.ToString(), "equal");

            // Negative test...
            var resultNestedNegative = RunAtomicQuery(evt, "extend comparison = iff(outsidetarget between(min..max), iff(nestedValue == 35, \"equal\", \"not equal\"), \"not between\")");
            var a = resultNestedNegative.ToArray();
            Assert.AreEqual(a[0].comparison.ToString(), "not between");
        }

        [TestMethod]
        public void DatePartExtend()
        {
            dynamic evt = new ExpandoObject();
            DateTime dt = Convert.ToDateTime("11/08/2017 14:50:50.42");
            evt.now = dt;

            var a = RunAtomicQuery(evt, "extend dt = datepart(\"Hour\",now)");
            Assert.AreEqual(Convert.ToInt64(a[0].dt.ToString()), 14);
            var b = RunAtomicQuery(evt, "extend dt = datepart(\"Minute\",now)");
            Assert.AreEqual(Convert.ToInt64(b[0].dt.ToString()), 50);
            var c = RunAtomicQuery(evt, "extend dt = datepart(\"Year\",now)");
            Assert.AreEqual(Convert.ToInt64(c[0].dt.ToString()), 2017);
            var d = RunAtomicQuery(evt, "extend dt = datepart(\"Day\",now)");
            Assert.AreEqual(Convert.ToInt64(d[0].dt.ToString()), 8);
            var e = RunAtomicQuery(evt, "extend dt = datepart(\"DayOfYear\",now)");
            Assert.AreEqual(Convert.ToInt64(e[0].dt.ToString()), 312);
            var f = RunAtomicQuery(evt, "extend dt = datepart(\"DayOfWeek\",now)");
            Assert.AreEqual(Convert.ToInt64(f[0].dt.ToString()), 3);
        }

        [TestMethod]
        public void BetweenFunction()
        {
            dynamic evt = new ExpandoObject();
            DateTime dt = Convert.ToDateTime("11/08/2017 14:50:50.42");
            evt.target = 14;
            evt.LowValue = 8;
            evt.HighValue = 18;
            evt.nowminustwo = dt.AddHours(-2);
            evt.now = dt;
            evt.nowplusfive = dt.AddHours(5);

            // TestWhere(evt, "datepart(\"Hour\",Today2pm) < 8 and datepart(\"Hour\",Today2pm) > 18)", true);
            TestWhere(evt, "target between (8 .. 18)", true);
            TestWhere(evt, "target !between (8 .. 18)", false);
            TestWhere(evt, "target between (LowValue .. HighValue)", true);

            TestWhere(evt, "now between (\"11/08/2017 12:50:50.42\" .. \"11/08/2017 19:50:50.42\")", true);
            TestWhere(evt, "now between (nowminustwo .. nowplusfive)", true);
            TestWhere(evt, "now !between (nowminustwo .. nowplusfive)", false);
        }

        [TestMethod]
        public void ExtendDictionaryFieldTest()
        {
            dynamic dict = new ExpandoObject();
            dict.Events = new List<object>
            {
                new
                {
                    Id = "12345"
                },
                new
                {
                    Id = "6789"
                },
                new
                {
                    Id = 87543
                }
            };

            dict.ObjectId = "13579";
            IDictionary<string, object> extend =
                new ExtendOperator("id1 = Events[0], id2 = Events[1].Id, id3 = Events[2].Id, obj = ObjectId").Extend(dict);

            Assert.AreEqual(extend["id2"], "6789");
            Assert.AreEqual(extend["id3"], 87543);
        }

        [TestMethod]
        public void ExtendDotIndexedNameSupportTest()
        {
            dynamic dict = new ExpandoObject() as IDictionary<string, object>;
            dict.One = "13579";
            dict.Two = 4432;

            ((IDictionary<string, object>) dict)["01"] = "13579";
            ((IDictionary<string, object>) dict)["02"] = 4432;

            dynamic evt = new ExpandoObject();
            evt.EventData = dict;

            IDictionary<string, object> extend =
                new ExtendOperator("Four = EventData.[\"01\"], Five = EventData.[\"02\"]").Extend(evt);

            Assert.AreEqual(extend["Four"], "13579");
            Assert.AreEqual(extend["Five"], 4432);
        }

        [TestMethod]
        public void ExtendIsNotNullTest()
        {
            dynamic dict = new ExpandoObject();
            dict.Id = 10;
            dict.Inner = new
            {
                Id = 20
            };

            var outerId = new WhereOperator("isnotnull(Id)").Evaluate(dict);
            var innerId = new WhereOperator("isnotnull(Inner.Id)").Evaluate(dict);

            Assert.AreEqual(outerId, true);
            Assert.AreEqual(innerId, true);
        }

        [TestMethod]
        public void ExtendIsNotNullListTest()
        {
            dynamic dict = new ExpandoObject();
            dict.Id = 10;
            dict.Inner = new List<object>
            {
                new
                {
                    Id = 20
                },
                "Foo"
            };

            var outerId = new WhereOperator("isnotnull(Id)").Evaluate(dict);
            var innerId = new WhereOperator("isnotnull(Inner[0].Id)").Evaluate(dict);

            Assert.AreEqual(outerId, true);
            Assert.AreEqual(innerId, true);
        }

        [TestMethod]
        public void ExtendDeeplyNestedObjectTest()
        {
            dynamic dict = new ExpandoObject();
            dict.original = new
            {
                Data = new
                {
                    NetworkDeviceName = "FRA20-0101-0103-17T0",
                    ReceivedDateTime = "2018-06-26T11:38:22.8758479Z",
                    Data = new
                    {
                        NetworkDeviceGroups = new List<object>
                        {
                            "Device Role:All Device Roles:NDG-PHY-FABRIC-T0",
                            "Program:All Programs:NDG-FABRICATOR-PROD",
                            "Location:All Locations",
                            "Property:All Properties:NDG-CIS",
                            "Device Type:All Device Types:NDG-ARISTA-ALL"
                        }
                    }
                }
            };

            int indexValue = 3;

            var extend =
                new ExtendOperator(
                    $"result = strcat(\"<br><strong>Data.NetworkDeviceGroups[{indexValue}]: </strong> \", original.Data.Data.NetworkDeviceGroups[{indexValue}])");
            IDictionary<string, object> result = extend.Extend(dict, true);

            bool containsValue = result["result"].ToString().Contains("Property:All Properties:NDG-CIS");
            Assert.IsTrue(containsValue);
        }

        [TestMethod]
        public void ExtendWithIndexedFieldReference()
        {
            dynamic testObject = new ExpandoObject();
            testObject.Rows = new List<Object>();

            dynamic row = new ExpandoObject();
            testObject.Rows.Add(row);
            row.SubscriptionId = Guid.Empty;

            ExtendOperator extendOperator = new ExtendOperator("SubscriptionId = Rows[0].SubscriptionId");
            dynamic e = extendOperator.Extend(testObject);
            object subscriptionId = e.SubscriptionId;
            Assert.IsTrue(!string.IsNullOrEmpty(subscriptionId?.ToString()), "Failed to expand SubscriptionId");
        }

        [TestMethod]
        public void ConditionOnNullElement()
        {
            dynamic testObject = new ExpandoObject();
            dynamic getVipHistoryWithConfirmation = new ExpandoObject();
            getVipHistoryWithConfirmation.SubscriptionId = Guid.Empty;
            getVipHistoryWithConfirmation.SubscriptionIdConfirmed = null;
            testObject.GetVipHistoryWithConfirmation = getVipHistoryWithConfirmation;

            var whereOperator = new WhereOperator(@"GetVipHistoryWithConfirmation.SubscriptionIdConfirmed == ""CONFIRMED""");
            var expression = whereOperator;

            bool value = expression.Evaluate(testObject);
            Assert.IsFalse(value, "Evaluation failed");

            var whereOperator1 = new WhereOperator(@"SubscriptionIdConfirmed == ""CONFIRMED""");
            var expression1 = whereOperator;

            value = expression1.Evaluate(getVipHistoryWithConfirmation);
            Assert.IsFalse(value, "Evaluation failed");
        }

        [TestMethod]
        public void ExtendPack()
        {
            dynamic evt = new ExpandoObject();
            evt.UnEqualCommandLine = "let's start a crazy process, and break the internet!";
            evt.EqualCommandLine = "this one should be equivalent!";
            evt.NumericSeed = 1111;

            List<string> listExtendTestScalarFunctions = new List<string>()
            {
                "extend x = pack(\"First\", \"Second\", \"Numeric\", 1234, \"Numeric2\", 4567)",
                //"extend text = strlen(\"1234567890\")",
                //"extend text = tostring(123)"
            };

            foreach (string extendedScalarFunction in listExtendTestScalarFunctions)
            {
                ExtendOperator extendOperator = new ExtendOperator(extendedScalarFunction);

                string serializationOfExtend = RxKqlCommonFunctions.ToJson(extendOperator);

                ExtendOperator hydratedExtendOperator = RxKqlCommonFunctions.ToExtendOperator(serializationOfExtend);

                Assert.AreEqual(extendedScalarFunction, hydratedExtendOperator.ToString());
            }

            // Positive test...
            var x = RunAtomicQuery(evt, "extend p = pack(\"First\", \"Second\", \"Numeric\", 1234, \"Numeric2\", 5678, \"FieldValueOfSeed\", NumericSeed)");
            Assert.AreEqual(x[0].p.First.ToString(), "Second"); // Should be "Second"
            Assert.AreEqual(x[0].p.Numeric.ToString(), "1234"); // Should be "1234"
            Assert.AreEqual(x[0].p.Numeric2.ToString(), "5678"); // Should be "5678"
            Assert.AreEqual(x[0].p.FieldValueOfSeed, 1111); // Should be "5678"
        }

        [TestMethod]
        public void ExtendPackArray()
        {
            dynamic evt = new ExpandoObject();
            evt.UnEqualCommandLine = "let's start a crazy process, and break the internet!";
            evt.EqualCommandLine = "this one should be equivalent!";
            evt.NumericSeed = 1111;

            List<string> listExtendTestScalarFunctions = new List<string>()
            {
                "extend x = packarray(\"First\", \"Second\", \"Numeric\", 1234, \"Numeric2\", 4567)",
                //"extend text = strlen(\"1234567890\")",
                //"extend text = tostring(123)"
            };

            foreach (string extendedScalarFunction in listExtendTestScalarFunctions)
            {
                ExtendOperator extendOperator = new ExtendOperator(extendedScalarFunction);

                string serializationOfExtend = RxKqlCommonFunctions.ToJson(extendOperator);

                ExtendOperator hydratedExtendOperator = RxKqlCommonFunctions.ToExtendOperator(serializationOfExtend);

                Assert.AreEqual(extendedScalarFunction, hydratedExtendOperator.ToString());
            }

            // Positive test...
            var x = RunAtomicQuery(evt, "extend pa = packarray(\"First\", \"Second\", \"Numeric\", 1234, \"Numeric2\", 5678, NumericSeed)");
            Assert.AreEqual(x[0].pa[0].ToString(), "First"); // Should be "First"
            Assert.AreEqual(x[0].pa[1].ToString(), "Second"); // Should be "Second"
            Assert.AreEqual(x[0].pa[2].ToString(), "Numeric"); // Should be "Numeric"
            Assert.AreEqual(x[0].pa[3], 1234); // Should be 1234
            Assert.AreEqual(x[0].pa[4].ToString(), "Numeric2"); // Should be "Numeric2"
            Assert.AreEqual(x[0].pa[5], 5678); // Should be 5678
            Assert.AreEqual(x[0].pa[6], 1111); // Should be 1111
        }
    }
}