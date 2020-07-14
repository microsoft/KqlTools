// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.UnitTest
{
    using Microsoft.EvtxEventXmlScrubber;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive.Kql;
    using System.Reflection;

    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void ProjectExtendValidation()
        {
            KqlNode node = new KqlNode();

            // Get the sample data for 4688 Process Create
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            string eventXmlOf4688 = File.ReadAllText(Path.Combine(directory, "ExampleEventXml", "Sample4688.xml"));

            List<KqlQuery> newDetections = new List<KqlQuery>();

            newDetections.Add(new KqlQuery
            {
                Comment = "This works",
                Query = "Security | where EventId == 4688 | extend ProcessName = EventData.NewProcessName | extend UserName = EventData.SubjectUserName | project ProcessName,UserName"
            });

            newDetections.Add(new KqlQuery
            {
                Comment = "This also should work",
                Query = "Security | where EventId == 4688 | project ProcessName = EventData.NewProcessName, UserName = EventData.SubjectUserName"
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the sucessful detections.
            var results = new List<IDictionary<string, object>>();

            node.Subscribe(evt => 
                { 
                    results.Add(evt.Output); 
            });

            // Add the detections.
            var eventDynamic = EvtxExtensions.Deserialize(eventXmlOf4688);
            node.OnNext((IDictionary<string, object>)eventDynamic);
            
            Assert.IsTrue(results.Count == 2);

            // Make sure BOTH return the same values
            var x = results.ToArray();
            string processNameValue = "C:\\Windows\\System32\\backgroundTaskHost.exe";
            string userNameValue = "RUSSELLHPDEV$";

            Assert.AreEqual(x[0]["ProcessName"], processNameValue);
            Assert.AreEqual(x[0]["UserName"], userNameValue);

            Assert.AreEqual(x[1]["ProcessName"], processNameValue);
            Assert.AreEqual(x[1]["UserName"], userNameValue);
        }

        [TestMethod]
        public void ProjectChainValidation()
        {
            KqlNode node = new KqlNode();

            // Get the sample data for 4688 Process Create
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            string eventXmlOf4688 = File.ReadAllText(Path.Combine(directory, "ExampleEventXml", "Sample4688.xml"));

            List<KqlQuery> newDetections = new List<KqlQuery>();
            
            newDetections.Add(new KqlQuery
            {
                Comment = "A Projected field from a projected field, piped projects...",
                Query = "Security " +
                        "| where EventId == 4688 " +
                        "| project ProcessName = EventData.NewProcessName, UserName = EventData.SubjectUserName " +
                        "| project newName = ProcessName"
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the sucessful detections.
            var results = new List<IDictionary<string, object>>();

            node.Subscribe(evt =>
            {
                results.Add(evt.Output);
            });

            // Add the detections.
            var eventDynamic = EvtxExtensions.Deserialize(eventXmlOf4688);
            node.OnNext((IDictionary<string, object>)eventDynamic);

            Assert.IsTrue(results.Count == 1);

            // Make sure BOTH return the same values
            var x = results.ToArray();
            string processNameValue = "C:\\Windows\\System32\\backgroundTaskHost.exe";

            Assert.AreEqual(x[0]["newName"], processNameValue);
        }

        [TestMethod]
        public void ProjectExtendChainedValidation()
        {
            KqlNode node = new KqlNode();

            // Get the sample data for 4688 Process Create
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            string eventXmlOf4688 = File.ReadAllText(Path.Combine(directory, "ExampleEventXml", "Sample4688.xml"));

            List<KqlQuery> newDetections = new List<KqlQuery>();

            newDetections.Add(new KqlQuery
            {
                Comment = "An extended projected ordeal...",
                Query = "Security " +
                        "| where EventId == 4688 " +
                        "| extend processName = EventData.NewProcessName" +
                        "| project newName = processName " +
                        "| extend extendedName = newName" +
                        "| project finalProcessNameResult = extendedName"
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the sucessful detections.
            var results = new List<IDictionary<string, object>>();

            node.Subscribe(evt =>
            {
                results.Add(evt.Output);
            });

            // Add the detections.
            var eventDynamic = EvtxExtensions.Deserialize(eventXmlOf4688);
            node.OnNext(eventDynamic);

            Assert.IsTrue(results.Count == 1);

            // Make sure BOTH return the same values
            var x = results.ToArray();
            string processNameValue = "C:\\Windows\\System32\\backgroundTaskHost.exe";

            Assert.AreEqual(x[0]["finalProcessNameResult"], processNameValue);
        }
    }
}