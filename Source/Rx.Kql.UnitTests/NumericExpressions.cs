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
    using System.Reactive.Kql;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class NumericExpressions : TestBase
    {
        [TestMethod]
        public void NumericExpressionTests()
        {
            string jsonText = "{\r\n  \"SrcIp\": \"12.34.45.245\"\r\n}";

            Dictionary<string, object> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);

            KqlNode node = new KqlNode();

            List<KqlQuery> newDetections = new List<KqlQuery>();

            newDetections.Add(new KqlQuery
            {
                Comment = "Determine Subnet",
                Query = "SIEMfx " +
                        "| extend SrcSubnet = substring(SrcIp, 0, strlen(SrcIp) - indexof(reverse(SrcIp), \".\")) " +
                        "| where SrcSubnet == \"12.34.45.\""
            });

            newDetections.Add(new KqlQuery
            {
                Comment = "Determine Subnet another way with extend statement calculation cascade",
                Query = "SIEMfx " +
                        "| extend three = toint(3) " +
                        "| extend four = 4 " +
                        "| extend SrcSubnet = substring(SrcIp, 0, (three * four) - three) " +
                        "| where SrcSubnet == \"12.34.45.\""
            });

            newDetections.Add(new KqlQuery
            {
                Comment = "Correctly calculate results based on valid PEMDAS",
                Query = "SIEMfx " +
                        "| extend five = 5 " +
                        "| extend two = 2 " +
                        "| extend three = five - two " +
                        "| extend fortyeight = 8 / 2 * (2+2) * three " +
                        "| where fortyeight == 48"
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the sucessful detections.
            var results = new List<IDictionary<string, object>>();

            node.Subscribe(evt => { results.Add(evt.Output); });

            // Add the execute on the simple IP.
            node.OnNext(jsonDictionary);

            // Make sure ALL of the above KqlQuery objects return detections
            Assert.IsTrue(results.Count == node.KqlQueryList.Count);
        }

        [TestMethod]
        public void DateTimeNumericExpressionTests()
        {
            string jsonText = "  {\r\n    \"FirstDate\": \"2018-12-10T13:45:00.000Z\",\r\n  \"SecondDate\": \"2018-12-10T14:45:00.000Z\"\r\n  }";

            Dictionary<string, object> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);

            KqlNode node = new KqlNode();

            List<KqlQuery> newDetections = new List<KqlQuery>();

            newDetections.Add(new KqlQuery
            {
                Comment = "Determine Subnet",
                Query = "SIEMfx " +
                        "| extend TimeDiff = SecondDate - FirstDate" +
                        "| where TimeDiff == \"01:00:00\""
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the successful detections.
            var results = new List<IDictionary<string, object>>();

            node.Subscribe(evt => { results.Add(evt.Output); });

            // Add the execute on the simple IP.
            node.OnNext(jsonDictionary);

            // Make sure ALL of the above KqlQuery objects return detections
            Assert.IsTrue(results.Count == node.KqlQueryList.Count);
        }

    }
}