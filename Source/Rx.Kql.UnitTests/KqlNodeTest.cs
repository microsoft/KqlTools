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
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Kql;
    using System.Reflection;
    using Microsoft.EvtxEventXmlScrubber;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class KqlNodeTest
    {
        [TestMethod]
        public void SimpleQueries()
        {
            KqlNode node = new KqlNode();

            // deserialize JSON to the runtime type, and iterate.
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            node.AddCslFile(Path.Combine(directory, "SimpleQueries.csl"));

            // Subscribe to the sucessful detections.
            var list = new List<object>();
            node.Subscribe(evt => { list.Add(evt); });

            // Add the detections.
            for (int i = 0; i < 10; i++)
            {
                dynamic evt = new ExpandoObject();
                evt.Seq = i;
                node.OnNext((IDictionary<string, object>) evt);
            }
        }

        [TestMethod]
        [TestCategory("RxKQL-KqlFunctionTest")]
        [Owner("Russell Biles")]
        public void KqlFunctionsAddRemoveDirect()
        {
            KqlNode node = new KqlNode();

            // deserialize JSON to the runtime type, and iterate.
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.Combine(Path.GetDirectoryName(path), "KqlFunctionTestFiles");

            // Get the FILTER files
            string supportedFileMaskCsl = "FILTER*.csl";
            var directoryInfo = new DirectoryInfo(directory);
            var orderedFileList =
                directoryInfo.EnumerateFiles(supportedFileMaskCsl, SearchOption.TopDirectoryOnly)
                    .Select(d => d.FullName)
                    .ToList();
            var sourceFileList = orderedFileList as IList<string> ?? orderedFileList.ToList();

            foreach (string file in sourceFileList)
            {
                string lines = File.ReadAllText(file);
                node.AddKqlFunction(lines);
            }

            // Assert all functions are added correctly
            Assert.AreEqual(GlobalFunctions.KqlFunctions.Count, sourceFileList.Count);

            // Make sure retrieval of a non-existant function doesn't throw an exception
            if (GlobalFunctions.KqlFunctions.Any())
            {
                CslFunction negativeTest = node.GetKqlFunction("NonExistentFunction");
                Assert.IsNull(negativeTest);
            }

            // Assert all functions are removed correctly
            int functionCounter = GlobalFunctions.KqlFunctions.Count;
            foreach (KeyValuePair<string, CslFunction> keyValuePair in GlobalFunctions.KqlFunctions)
            {
                node.RemoveKqlFunction(keyValuePair.Key);
                functionCounter--;
                Assert.AreEqual(GlobalFunctions.KqlFunctions.Count, functionCounter);
            }
        }

        [TestMethod]
        public void FunctionQueries()
        {
            KqlNode node = new KqlNode();

            // deserialize JSON to the runtime type, and iterate.
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            node.AddCslFile(Path.Combine(directory, "KqlFunctionTestFiles", "Rule_4720_UsrAcctCreation_WecExtract.csl"));

            Debug.Assert(GlobalFunctions.KqlFunctions.Count == 3, "Rx.Kql FILTER Functions are not loading correctly from CSL files!");
            Debug.Assert(node.KqlQueryList.Count > 0 || node.FailedKqlQueryList.Count == 0, "Kql query failed to load.  There is an Rx.Kql parsing bug!");

            string evt4720 =
                "<Event xmlns=\'http://schemas.microsoft.com/win/2004/08/events/event\' xml:lang=\'en-US\'><System><Provider Name=\'Microsoft-Windows-Security-Auditing\' Guid=\'{54849625-5478-4994-A5BA-3E3B0328C30D}\'/><EventID>4720</EventID><Version>0</Version><Level>0</Level><Task>13824</Task><Opcode>0</Opcode><Keywords>0x8020000000000000</Keywords><TimeCreated SystemTime=\'2017-08-31T19:38:21.509585500Z\'/><EventRecordID>2079336</EventRecordID><Correlation/><Execution ProcessID=\'2092\' ThreadID=\'42656\'/><Channel>Security</Channel><Computer>SN2SCH101140124.phx.gbl</Computer><Security/></System><EventData><Data Name=\'TargetUserName\'>QTU-bs_el_idsv-7</Data><Data Name=\'TargetDomainName\'>SN2SCH101140124</Data><Data Name=\'TargetSid\'>S-1-5-21-1266794097-2621680504-1140025688-1442</Data><Data Name=\'SubjectUserSid\'>S-1-5-21-606747145-1563985344-839522115-25776942</Data><Data Name=\'SubjectUserName\'>_qcloud1</Data><Data Name=\'SubjectDomainName\'>PHX</Data><Data Name=\'SubjectLogonId\'>0x21a3d239e</Data><Data Name=\'PrivilegeList\'>-</Data><Data Name=\'SamAccountName\'>QTU-bs_el_idsv-7</Data><Data Name=\'DisplayName\'>%%1793</Data><Data Name=\'UserPrincipalName\'>-</Data><Data Name=\'HomeDirectory\'>%%1793</Data><Data Name=\'HomePath\'>%%1793</Data><Data Name=\'ScriptPath\'>%%1793</Data><Data Name=\'ProfilePath\'>%%1793</Data><Data Name=\'UserWorkstations\'>%%1793</Data><Data Name=\'PasswordLastSet\'>%%1794</Data><Data Name=\'AccountExpires\'>%%1794</Data><Data Name=\'PrimaryGroupId\'>513</Data><Data Name=\'AllowedToDelegateTo\'>-</Data><Data Name=\'OldUacValue\'>0x0</Data><Data Name=\'NewUacValue\'>0x15</Data><Data Name=\'UserAccountControl\'>\r\n\t\t%%2080\r\n\t\t%%2082\r\n\t\t%%2084</Data><Data Name=\'UserParameters\'>%%1793</Data><Data Name=\'SidHistory\'>-</Data><Data Name=\'LogonHours\'>%%1797</Data></EventData></Event>";

            dynamic eventDynamic = EvtxExtensions.Deserialize(evt4720);

            // Subscribe to the sucessful detections.
            var list = new List<object>();
            node.Subscribe(evt => { list.Add(evt); });

            node.OnNext((IDictionary<string, object>) eventDynamic);
        }

        [TestMethod]
        public void DetectionInfoQueries()
        {
            KqlNode node = new KqlNode();

            // deserialize JSON to the runtime type, and iterate.
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);

            List<KqlQuery> newDetectionInfos = new List<KqlQuery>();
            newDetectionInfos.Add(new KqlQuery
            {
                Comment = "// GLOBAL - 1102 - Audit Log Cleared - SIM-00014",
                Query =
                    "SecurityLog | where EventId == 1102 | extend SubjectUserName = EventData.SubjectUserName, SubjectDomainName = EventData.SubjectDomainName | project TimeCreated, Computer, SubjectUserName, SubjectDomainName"
            });

            node.AddKqlQueryList(newDetectionInfos, true);

            // Subscribe to the sucessful detections.
            var list = new List<object>();

            node.Subscribe(evt => { list.Add(evt); });

            string eventXmlOf1102 =
                "<Event xmlns='http://schemas.microsoft.com/win/2004/08/events/event' xml:lang='en-US'><System><Provider Name='Microsoft-Windows-Eventlog' Guid='{fc65ddd8-d6ef-4962-83d5-6e5cfe9ce148}'/><EventID>1102</EventID><Version>0</Version><Level>4</Level><Task>104</Task><Opcode>0</Opcode><Keywords>0x4020000000000000</Keywords><TimeCreated SystemTime='2017-08-03T17:11:29.255592600Z'/><EventRecordID>36837151</EventRecordID><Correlation/><Execution ProcessID='996' ThreadID='11180'/><Channel>Security</Channel><Computer>GFTVMHostDev.redmond.corp.microsoft.com</Computer><Security/></System><UserData><LogFileCleared xmlns='http://manifests.microsoft.com/win/2004/08/windows/eventlog'><SubjectUserSid>S-1-5-21-2127521184-1604012920-1887927527-9916173</SubjectUserSid><SubjectUserName>rbiles</SubjectUserName><SubjectDomainName>REDMOND</SubjectDomainName><SubjectLogonId>0x34d1b1eb</SubjectLogonId></LogFileCleared></UserData></Event>";

            // Add the detections.
            var eventDynamic = EvtxExtensions.Deserialize(eventXmlOf1102);
            node.OnNext(eventDynamic);
        }

        [TestMethod]
        public void AddingDetectionMultipleItems()
        {
            KqlNode node = new KqlNode();

            List<KqlQuery> newDetectionInfos = new List<KqlQuery>();
            newDetectionInfos.Add(new KqlQuery
            {
                Comment = "// GLOBAL - 1102 - Audit Log Cleared - SIM-00014",
                Query =
                    "SecurityLog | where EventId == 1102 | extend SubjectUserName = EventData.SubjectUserName, SubjectDomainName = EventData.SubjectDomainName | project TimeCreated, Computer, SubjectUserName, SubjectDomainName"
            });
            newDetectionInfos.Add(new KqlQuery
            {
                Comment = "// GLOBAL - 1103 ",
                Query = "SecurityLog | where EventId == 1103"
            });

            node.AddKqlQueryList(newDetectionInfos, true);
            if (node.KqlQueryList.Count != 2)
            {
                throw new Exception();
            }
        }

        [TestMethod]
        public void RandomQueryDetectionInfo()
        {
            KqlNode node = new KqlNode();

            KqlQuery detectionItem = new KqlQuery
            {
                Comment = "// GLOBAL - 1102 - Audit Log Cleared - SIM-00014",
                Query =
                    "cluster('CDOCC').database('WEC').SecurityLog | where Provider == 'Microsoft-Windows-Security-Auditing' and EventId == 4728 | extend MemberName = EventData.MemberName, MemberSid = EventData.MemberSid, TargetUserName = EventData.TargetUserName | where MemberName contains 'Domain Computers' or (MemberSid startswith 'S-1-5-21-' and MemberSid endswith '-515') or MemberName contains 'Domain Users' or (MemberSid startswith 'S-1-5-21-' and MemberSid endswith '-513')"
            };

            Query q = new Query(detectionItem.Query);

            List<KqlQuery> newDetectionInfos = new List<KqlQuery>();
            newDetectionInfos.Add(detectionItem);
            node.AddKqlQueryList(newDetectionInfos, true);
        }

        [TestMethod]
        public void ValidateIndexedFieldReference()
        {
            KqlNode node = new KqlNode();

            // Get the sample data for 1116 AntiMalware
            var path = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(path);
            string eventXmlOf1116AntiMalware = File.ReadAllText(Path.Combine(directory, "ExampleEventXml", "1116_AntiMalware.xml"));

            string comment =
                "/\'DDID 543: SIM-00035: GLOBAL - 1116-1119 - Microsoft Antimalware\', folder = @\'DetectionFunctionBuildout/WEC/Production/Global\')";

            string query =
                "cluster(\"CDOC\").database(\"WEC\").SecurityLog\r\n    | where Provider == \"Microsoft Antimalware\" and EventId in (1116, 1117, 1118, 1119)\r\n    | extend   FilePath = EventData.[\"22\"], SourceProcessName = EventData.[\"19\"], SourceUserName = EventData.[\"20\"], ThreatName = EventData.[\"08\"] \r\n    | where not(ThreatName endswith \"EICAR_Test_File\" and Computer startswith \"WU2SGRPVT\")\r\n\t| where not(ThreatName == \"Trojan:Win32/Peals.F!cl\")\r\n\t| where not(SourceProcessName contains \":\\\\data\\\\Perf\\\\\" or FilePath contains \":\\\\data\\\\Perf\\\\\")\r\n    | where not((FilePath contains \"C:\\\\Users\\\\L4Test\" or FilePath contains \"D:\\\\Users\\\\L4Test\") and SourceUserName startswith \"L4TestUser\")\r\n    | where not(FilePath contains \"NGFMuploads\" and Computer in (\"CO1MSSDTMLFS11.phx.gbl\", \"CO1MSSDTMLFS12.phx.gbl\", \"CO1MSSDTMLFS13.phx.gbl\", \"CO1MSSDTMLFS14.phx.gbl\", \"DB3MSSDTMLFS11.phx.gbl\", \"DB3MSSDTMLFS12.phx.gbl\", \"DB3MSSDTMLFS13.phx.gbl\", \"DB3MSSDTMLFS14.phx.gbl\", \"SG2MSSDTMLFS11.phx.gbl\", \"SG2MSSDTMLFS12.phx.gbl\", \"SG2MSSDTMLFS13.phx.gbl\", \"SG2MSSDTMLFS14.phx.gbl\"))\r\n    | where not(FilePath startswith \"file:_D:\\\\http\\\\security\\\\encyclopedia\\\\en-us\\\\entries\\\\\" or FilePath startswith \"file:_E:\\\\Services\\\\HostIDS\\\\\" or FilePath startswith \"file:_E:\\\\AzureMAStore\\\\\" or FilePath startswith \"file:_E:\\\\Services\\\\WLS_Colorado\\\\\" or FilePath startswith \"file:_C:\\\\Windows\\\\System32\\\\config\\\\systemprofile\\\\AppData\\\\Local\\\\Microsoft\\\\Windows\\\\\")\r\n    | where not(SourceProcessName endswith \"agent\\\\MonAgentCore.exe\" or SourceProcessName endswith \"service\\\\MonAgentCore.exe\")\r\n    | where not((FilePath contains \":\\\\Temp\\\\Website\\\\\" and Computer contains \"MSDN\") or FilePath contains \"\\\\Users\\\\YarnppNMUser\\\\AppData\\\\Local\\\\Temp\\\\\")\r\n";

            List<KqlQuery> newDetections = new List<KqlQuery>();
            newDetections.Add(new KqlQuery
            {
                Comment = comment,
                Query = query
            });

            node.AddKqlQueryList(newDetections, true);

            // Subscribe to the sucessful detections.
            var list = new List<object>();

            node.Subscribe(evt => { list.Add(evt); });

            // Add the detections.
            var eventDynamic = EvtxExtensions.Deserialize(eventXmlOf1116AntiMalware);
            node.OnNext(eventDynamic);
        }
    }
}