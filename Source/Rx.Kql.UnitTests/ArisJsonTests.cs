// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace Rx.Kql.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reactive.Kql;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ArisJsonTests : TestBase
    {
        // Tests of various Kusto operators described here:
        // https://kusto.azurewebsites.net/docs/queryLanguage/concepts_datatypes_string_operators.html
        [TestMethod]
        public void NestedJsonMissingField()
        {
            string jsonText =
                "\r\n{\r\n  \"ObjectProperty\": null,\r\n  \"original\": {\r\n    \"Source\": \"Kusto_CDOC\",\r\n    \"OccurenceUtc\": \"2018-09-05T19:16:07Z\",\r\n    \"Origin\": {},\r\n    \"ArisId\": \"bcb5be12-c55d-43ab-86bf-d5b5276267ff\",\r\n    \"Name\": \"CiscoAAAExternalAuthSuccessfromIPv6\",\r\n    \"Data\": {\r\n      \"Data\": {\r\n        \"NetworkDeviceName\": \"icr01.hkg20\",\r\n        \"SelectedAuthenticationIdentityStores\": [\r\n          \"Internal Users\",\r\n          \"AD1\",\r\n          \"Internal Users\",\r\n          \"AD1\"\r\n        ],\r\n        \"AuthorizationPolicyMatchedRule\": \"NWK-GNS-MSG-ALLDVC-RO\",\r\n        \"ServiceSelectionMatchedRule\": \"JUNIPER-ALL\",\r\n        \"AuthenticationIdentityStore\": \"AD1\",\r\n        \"AD-User-Resolved-Identities\": \"khoih@gme.gbl\",\r\n        \"IdentityPolicyMatchedRule\": \"JUNIPER-ALL\",\r\n        \"IdentityAccessRestricted\": \"false\",\r\n        \"UserName\": \"khoih\",\r\n        \"SelectedAccessService\": \"JUNIPER-ALL\",\r\n        \"AD-User-NetBios-Name\": \"GME\",\r\n        \"DestinationIPAddress\": \"25.248.254.30\",\r\n        \"AuthenticationMethod\": \"PAP_ASCII\",\r\n        \"AD-User-Resolved-DNs\": \"CN=khoih\\\\,OU=ServiceAccounts\\\\,DC=gme\\\\,DC=gbl\",\r\n        \"SelectedShellProfile\": \"JUNIPER-JUNOS-TOOLS\",\r\n        \"Hostname\": \"azusscseclinutil01-aaa-01\",\r\n        \"NetworkDeviceGroups\": [\r\n          \"Device Role:All Device Roles:NDG-CORE\",\r\n          \"Program:All Programs:NDG-CORE20\",\r\n          \"Location:All Locations\",\r\n          \"Property:All Properties\",\r\n          \"Device Type:All Device Types:NDG-JUNIPER-JUNOS\"\r\n        ],\r\n        \"AD-User-Join-Point\": \"GME.GBL\",\r\n        \"AD-User-DNS-Domain\": \"gme.gbl\",\r\n        \"Device IP Address\": \"10.109.252.10\",\r\n        \"SequenceNumber\": \"0\",\r\n        \"AD-Error-Details\": \"Domain trust is one-way\",\r\n        \"ConfigVersionId\": \"13968\",\r\n        \"DestinationPort\": \"49\",\r\n        \"Privilege-Level\": \"1\",\r\n        \"AD-Groups-Names\": \"gme.gbl/SecurityGroups/NWK-SVCACCTS-ALLDVC-RO\",\r\n        \"Remote-Address\": \"2001:506:28:16:f96e:e01d:7832:283a\",\r\n        \"RequestLatency\": \"15\",\r\n        \"ExternalGroups\": \"gme.gbl/SecurityGroups/NWK-SVCACCTS-ALLDVC-RO\",\r\n        \"SerialNumber\": \"0287167400\",\r\n        \"AcsSessionID\": \"osa01-aaa-01/322609966/88313513\",\r\n        \"Authen-Type\": \"ASCII\",\r\n        \"ACSVersion\": \"acs-5.8.0.32-B.442.x86_64\",\r\n        \"AD-Domain\": \"gme.gbl\",\r\n        \"Protocol\": \"Tacacs\",\r\n        \"MetaData\": \"2018-08-30 21:06:25.659 +00:00 1588549455 5201 NOTICE Test detection from khoih Passed-Authentication: Authentication succeeded\",\r\n        \"StepData\": [\r\n          \"19=khoih\",\r\n          \"20=gme.gbl\",\r\n          \"21=gme.gbl\",\r\n          \"22=onestore.gbl\\\\,Domain trust is one-way\",\r\n          \"24=khoih@gme.gbl\",\r\n          \"27=gme.gbl\"\r\n        ],\r\n        \"Category\": \"CSCOacs_Passed_Authentications\",\r\n        \"Response\": \"{Type=Authentication; Authen-Reply-Status=Pass; }\",\r\n        \"Service\": \"Login\",\r\n        \"Action\": \"Login\",\r\n        \"Type\": \"Authentication\",\r\n        \"User\": \"khoih\",\r\n        \"Step\": [\r\n          \"13013\",\r\n          \"15008\",\r\n          \"15004\",\r\n          \"15012\",\r\n          \"15041\",\r\n          \"15004\",\r\n          \"15013\",\r\n          \"24210\",\r\n          \"24216\",\r\n          \"13045\",\r\n          \"13015\",\r\n          \"13014\",\r\n          \"15037\",\r\n          \"15041\",\r\n          \"15004\",\r\n          \"15013\",\r\n          \"24210\",\r\n          \"24216\",\r\n          \"24430\",\r\n          \"24325\",\r\n          \"24313 ,Step=24319\",\r\n          \"24367\",\r\n          \"24323\",\r\n          \"24343\",\r\n          \"24402\",\r\n          \"24432\",\r\n          \"24355\",\r\n          \"24416\",\r\n          \"22037\",\r\n          \"15044\",\r\n          \"15035\",\r\n          \"15042\",\r\n          \"15036\",\r\n          \"15004\",\r\n          \"13015\"\r\n        ],\r\n        \"Port\": \"\"\r\n      },\r\n      \"IngestionTime\": \"2018-09-05T19:13:13.7065501Z\",\r\n      \"SourceIPAddress\": \"10.14.18.20\",\r\n      \"ArisReportSource\": \"CDOCKusto\",\r\n      \"NetworkDeviceName\": \"icr01.hkg20\",\r\n      \"Severity\": \"Warning\",\r\n      \"UserName\": \"khoih\",\r\n      \"ReceivedDateTime\": \"2018-09-05T19:07:23.1501086Z\",\r\n      \"Hostname\": \"azusscseclinutil01-aaa-01\",\r\n      \"DestinationAddress\": \"10.109.252.10\",\r\n      \"DetectionID\": \"871\",\r\n      \"IPv6ColonCount\": 7,\r\n      \"SourceAddress\": \"2001:506:28:16:f96e:e01d:7832:283a\",\r\n      \"Facility\": \"Local1\",\r\n      \"Message\": \"Sep 05 19:07:23 azusscseclinutil01-aaa-01 CSCOacs_Passed_Authentications 0287167400 3 0 2018-08-30 21:06:25.659 +00:00 1588549455 5201 NOTICE Test detection from khoih Passed-Authentication: Authentication succeeded, ACSVersion=acs-5.8.0.32-B.442.x86_64, ConfigVersionId=13968, Device IP Address=10.109.252.10, DestinationIPAddress=25.248.254.30, DestinationPort=49, UserName=khoih, Protocol=Tacacs, RequestLatency=15, Type=Authentication, Action=Login, Privilege-Level=1, Authen-Type=ASCII, Service=Login, User=khoih, Port=, Remote-Address=2001:506:28:16:f96e:e01d:7832:283a, UserName=khoih, AcsSessionID=osa01-aaa-01/322609966/88313513, AuthenticationIdentityStore=AD1, AuthenticationMethod=PAP_ASCII, SelectedAccessService=JUNIPER-ALL, SelectedShellProfile=JUNIPER-JUNOS-TOOLS, Step=13013 , Step=15008 , Step=15004 , Step=15012 , Step=15041 , Step=15004 , Step=15013 , Step=24210 , Step=24216 , Step=13045 , Step=13015 , Step=13014 , Step=15037 , Step=15041 , Step=15004 , Step=15013 , Step=24210 , Step=24216 , Step=24430 , Step=24325 , Step=24313 ,Step=24319 , Step=24367 , Step=24323 , Step=24343 , Step=24402 , Step=24432 , Step=24355 , Step=24416 , Step=22037 , Step=15044 , Step=15035 , Step=15042 , Step=15036 , Step=15004 , Step=13015 , SelectedAuthenticationIdentityStores=Internal Users, SelectedAuthenticationIdentityStores=AD1, SelectedAuthenticationIdentityStores=Internal Users, SelectedAuthenticationIdentityStores=AD1, NetworkDeviceName=icr01.hkg20, NetworkDeviceGroups=Device Role:All Device Roles:NDG-CORE, NetworkDeviceGroups=Program:All Programs:NDG-CORE20, NetworkDeviceGroups=Location:All Locations, NetworkDeviceGroups=Property:All Properties, NetworkDeviceGroups=Device Type:All Device Types:NDG-JUNIPER-JUNOS, ServiceSelectionMatchedRule=JUNIPER-ALL, IdentityPolicyMatchedRule=JUNIPER-ALL, AuthorizationPolicyMatchedRule=NWK-GNS-MSG-ALLDVC-RO, Action=Login, Privilege-Level=1, Authen-Type=ASCII, Service=Login, Port=, Remote-Address=2001:506:28:16:f96e:e01d:7832:283a,AD-User-Candidate-Identities=khoih@gme.gbl, AD-User-DNS-Domain=gme.gbl, AD-User-NetBios-Name=GME, AD-User-Resolved-Identities=khoih@gme.gbl, AD-User-Join-Point=GME.GBL, AD-User-Resolved-DNs=CN=khoih\\\\,OU=ServiceAccounts\\\\,DC=gme\\\\,DC=gbl, AD-Groups-Names=gme.gbl/SecurityGroups/NWK-SVCACCTS-ALLDVC-RO, AD-Error-Details=Domain trust is one-way, StepData=19=khoih, StepData=20=gme.gbl, StepData=21=gme.gbl, StepData=22=onestore.gbl\\\\,Domain trust is one-way, StepData=24=khoih@gme.gbl, StepData=27=gme.gbl, AD-Domain=gme.gbl, ExternalGroups=gme.gbl/SecurityGroups/NWK-SVCACCTS-ALLDVC-RO, IdentityAccessRestricted=false, Response={Type=Authentication; Authen-Reply-Status=Pass; }\",\r\n      \"Lineage\": {\r\n        \"SourceGuid\": \"aca57a6a-7608-4ef9-8533-07d7cb94b61d\",\r\n        \"ServerName\": \"syslogrx01\"\r\n      }\r\n    }\r\n  }\r\n}\r\n\r\n";

            Dictionary<string, object> jsonDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonText);

            string strcatHtml =
                "extend result = strcat(ObjectProperty, \"<br><strong>ArisReportSource: </strong>\", original.Source, \"<br><strong>DetectionId: </strong>\", original.Data.DetectionID, \"<br><strong>Data.ACSVersion: </strong>\", original.Data.Data.ACSVersion, \"<br><strong>Data.Action: </strong>\", original.Data.Data.Action, \"<br><strong>Data.Category: </strong>\", original.Data.Data.Category, \"<br><strong>UserName: </strong>\", original.Data.UserName, \"<br><strong>SourceAddress: </strong>\", original.Data.SourceAddress, \"<br><strong>DestinationAddress: </strong>\", original.Data.DestinationAddress, \"<br><strong>Data.Hostname: </strong>\", original.Data.Data.Hostname, \"<br><strong>Data.Device IP Address: </strong>\", original.Data.Data.[\"Device IP Address\"], \"<br><strong>Data.Remote-Address: </strong>\", original.Data.Data.[\"Remote-Address\"], \"<br><strong>Data.SelectedShellProfile: </strong>\", original.Data.Data.SelectedShellProfile, \"<br><strong>Data.UserName: </strong>\", original.Data.Data.UserName, \"<br><strong>Data.AD-Groups-Names: </strong>\", original.Data.Data.[\"AD-Groups-Names\"], \"<br><strong>Data.AD-User-Candidate-Identities: </strong>\", original.Data.Data.[\"AD-User-Candidate-Identities\"], \"<br><strong>Data.NetworkDeviceGroups[0]: </strong>\", original.Data.Data.NetworkDeviceGroups[0], \"<br><strong>Data.NetworkDeviceName: </strong>\", original.Data.Data.NetworkDeviceName, \"<br><strong>Message: </strong>\", original.Data.Message, \"<br><strong>IngestionTime: </strong>\", original.Data.IngestionTime, \"<br><strong>ReceivedDateTime: </strong>\", original.Data.ReceivedDateTime, \"<br>\") ";

            string strcatHtmlWithSpaces =
                "extend result = strcat(ObjectProperty, \" <br><strong>ArisReportSource: </strong> \", original.Source, \" <br><strong>DetectionId: </strong> \", original.Data.DetectionID, \" <br><strong>Data.ACSVersion: </strong> \", original.Data.Data.ACSVersion, \" <br><strong>Data.Action: </strong> \", original.Data.Data.Action, \" <br><strong>Data.Category: </strong> \", original.Data.Data.Category, \" <br><strong>UserName: </strong> \", original.Data.UserName, \" <br><strong>SourceAddress: </strong> \", original.Data.SourceAddress, \" <br><strong>DestinationAddress: </strong> \", original.Data.DestinationAddress, \" <br><strong>Data.Hostname: </strong> \", original.Data.Data.Hostname, \" <br><strong>Data.Device IP Address: </strong> \", original.Data.Data.[\"Device IP Address\"], \" <br><strong>Data.Remote-Address: </strong> \", original.Data.Data.[\"Remote-Address\"], \" <br><strong>Data.SelectedShellProfile: </strong> \", original.Data.Data.SelectedShellProfile, \" <br><strong>Data.UserName: </strong> \", original.Data.Data.UserName, \" <br><strong>Data.AD-Groups-Names: </strong> \", original.Data.Data.[\"AD-Groups-Names\"], \" <br><strong>Data.AD-User-Candidate-Identities: </strong> \", original.Data.Data.[\"AD-User-Candidate-Identities\"], \" <br><strong>Data.NetworkDeviceGroups[0]: </strong> \", original.Data.Data.NetworkDeviceGroups[0], \" <br><strong>Data.NetworkDeviceName: </strong> \", original.Data.Data.NetworkDeviceName, \" <br><strong>Message: </strong> \", original.Data.Message, \" <br><strong>IngestionTime: </strong> \", original.Data.IngestionTime, \" <br><strong>ReceivedDateTime: </strong> \", original.Data.ReceivedDateTime, \" <br>\") ";

            var e = new ExtendOperator(strcatHtml).Extend(jsonDictionary);
            var eEx = new ExtendOperator(strcatHtmlWithSpaces).Extend(jsonDictionary);
            //Assert.AreEqual(e.result, "The first field <br><strong>ArisReportSource: </strong>  <br><strong>ArisReportSource: </strong> ");
            //Assert.AreEqual(eEx.result, "The first field <br><strong>ArisReportSource: </strong>  <br><strong>ArisReportSource: </strong> ");
        }

        [TestMethod]
        [TestCategory("RxKQL")]
        [Owner("Dan Nicolescu")]
        public void WhereTest_DecisionOnTree()
        {
            string dataJson = @"{
                ""Source"": ""WEC-CDOC"",
                ""OccurenceUtc"": ""2018-09-11T22:22:13.0000000Z"",
                ""Origin"": {},
                ""ArisId"": ""f57a0c50-9326-409d-85a4-f15aca0e7392"",
                ""Name"": ""GME Smartcard Service Accounts - SIM-00011"",
                ""Data"": {
                  ""detectionDictionaryData_RxKqlDetectionItemEntityId"": ""1414"",
                  ""Rows"": [
                            {""IcmTeam"" : ""a""},
                            {""IcmTeam"" : ""b""},
                          ],
                }
              }";

            var expando = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);

            try
            {
                string whereExpression = @"Data.Rows[0].IcmTeam == ""a""";
                var whereOperator = new WhereOperator(whereExpression);
                var result = whereOperator.Evaluate(expando);
                Assert.IsTrue(result);

                whereExpression = @"Data.Rows[1].IcmTeam == ""b""";
                whereOperator = new WhereOperator(whereExpression);
                result = whereOperator.Evaluate(expando);
                Assert.IsTrue(result);

                whereExpression = @"Data.Rows[0].IcmTeam != ""b""";
                whereOperator = new WhereOperator(whereExpression);
                result = whereOperator.Evaluate(expando);
                Assert.IsTrue(result);

                whereExpression = @"Data.Rows[1].IcmTeam != ""b""";
                whereOperator = new WhereOperator(whereExpression);
                result = whereOperator.Evaluate(expando);
                Assert.IsFalse(result);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Extend Failed: {exception}");
            }
        }

        [TestMethod]
        [TestCategory("RxKQL")]
        [Owner("Russell Biles")]
        public void WhereTest_MissingField()
        {
            string dataJson = @"{
                ""Source"": ""WEC-CDOC"",
                ""OccurenceUtc"": ""2018-09-11T22:22:13.0000000Z"",
                ""Origin"": {},
                ""ArisId"": ""f57a0c50-9326-409d-85a4-f15aca0e7392"",
                ""Name"": ""GME Smartcard Service Accounts - SIM-00011"",
                ""Data"": {
                  ""detectionDictionaryData_RxKqlDetectionItemEntityId"": ""1414"",
                  ""Rows"": [
                            {""IcmTeam"" : ""a""},
                            {""IcmTeam"" : ""b""},
                          ],
                }
              }";

            var expando = JsonConvert.DeserializeObject<ExpandoObject>(dataJson);

            string extendExpression = @"extend Missing =  Data.Rows[0].MissingField";
            IDictionary<string, object> eEx = new ExtendOperator(extendExpression).Extend(expando);

            Console.WriteLine(JsonConvert.SerializeObject(eEx));
        }

        [TestMethod]
        [TestCategory("RxKQL")]
        [Owner("Ying Qian")]
        public void ArisExtendOperatorWithHtml()
        {
            string descriptionMarkdown = "\"<br>**{TicketTitle}**<br><br> A binary / script was loaded that violates RDOS Host Code Integrity (CI)policy.This may be due to a signing error, but is also a * *common indicator of malicious activity * *.The file load was not blocked as the host is currently in CI audit - only mode.Please review the attached file for details.Loading will be blocked when the host is switched to CI enforcement mode.<br>                        < span style = \'color:red; background:yellow\' > If you don’t recognize the violating file or the activity is suspicious, do **NOT * * try to investigate further but instead immediately escalate to CDOC using the procedure described below.</ span ><br>                               *Event Date / Time: { original.Data.AlertDateTimeUTC}<br>            *Event ID: { original.Data.EventId}<br>            *Policy name: { original.Data.PolicyName}<br>            *Policy version: { original.Data.PolicyVersion}<br>            **Location of Violation:**<br>            -Hashlist Report: [report csv]({ GetFileHashReport.Location})<br>                -Number of Rows: { GetFileHashReport.NumberOfRows}<br>            **Violating file: **<br>             *File name: { original.Data.FileName}<br>            *File path: { original.Data.FilePath}<br>            *File hash: { original.Data.FileHash}<br>            *Count of Machines: { original.Data.CountOfMachines}<br>            *Parent process name: { original.Data.ProcessName}<br>            *Parent process path: { original.Data.ProcessPath}<br>            *Service Namespace: { original.Data.ServiceNamespace}<br>            *Plugin:  { original.Data.Plugin}<br>                < span style = \'background:yellow\' > **HOW DO I RESOLVE THIS ALERT * *</ span ><br><br>                     The most likely reason for this alert is that the file is unsigned or signed by a publisher that is not trusted by Code Integrity policy.Refer to the[Code sign wiki](https://msazure.visualstudio.com/OneBranch/_wiki?pagePath=/Codesign) page for info on correctly signing your files. If the files are created by another team or this was misrouted to your team, please reassign the ticket to CDOC so they can get it properly routed to that property.<br>                     < span style = \'color:red\' > If you don’t recognize the violating file or the activity is suspicious, do **NOT * * try to investigate further but instead immediately escalate to CDOC using the following procedure: **Escalate ICM ticket to C+E Security Triage**<br>                            < span style = \'background:yellow\' > **How do I get help * *</ span ><br>                                 < span style = \'background:yellow\' > If the issue wasn’t simply due to an unsigned file, use the **[RDOS Host Code Integrity TSG](https://aka.ms/rdos-codeintegrity-tsg)** to troubleshoot and remediate the incident.</span><br>                                      < span style = \'background:yellow\' > If you need build help please visit the CloudES build help site: [https://microsoft.sharepoint.com/teams/WAG/EngSys/Implement/OneBranch/Home.aspx](https://microsoft.sharepoint.com/teams/WAG/EngSys/Implement/OneBranch/Home.aspx) .</span><br>                                      < span style = \'background:yellow\' > For more information or help, please email: [AzSysLockHelp@microsoft.com](mailto: AzSysLockHelp@microsoft.com).</ span >\"";
            if (string.IsNullOrEmpty(descriptionMarkdown))
            {
                return;
            }

            dynamic mdContent = new ExpandoObject();
            // mdContent.Text = Kusto.Data.Common.CslStringLiteral.AsCslString(descriptionMarkdown);

            mdContent.Text = descriptionMarkdown;

            string preExtendedStmt = $"extend result = {descriptionMarkdown}";

            // Commented due to AsCslString escapes content which corrupts HTML content, fix pending for escaped quoted strings with the above complexity
            IDictionary < string, object > e = new ExtendOperator(preExtendedStmt).Extend(mdContent);

            ExtendOperator eo = new ExtendOperator(preExtendedStmt);

            Assert.AreEqual(e["Text"].ToString().TrimEdges(new List<string> { "\""}, StringComparison.InvariantCultureIgnoreCase), e["result"]);
        }
    }
}