// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System.Dynamic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Rx.Kql.UnitTests.FunctionsTest
{
    [TestClass]
    public class StringArrayOperatorTests : TestBase
    {
        [TestMethod]
        public void StringOperatorToUpperOnGenericList()
        {
            // The JSON of the event to detect
            string eventJsonString = "{\r\n  \"ComputerName\": {\r\n    \"Conflux\": {\r\n      \"AssetExtended\": {\r\n        \"AzureSubscriptionId\": \"\",\r\n        \"STServiceGroupOid\": \"febd1b2f-5c1b-4d49-939c-c1fbe0c5849c\",\r\n        \"HasAVException\": 0,\r\n        \"ServiceName\": \"Support.microsoft.com (SMC)\",\r\n        \"IsAutoPilot\": 0,\r\n        \"DomainCalc\": \"\",\r\n        \"ServiceOid\": \"453f558d-0036-4db7-9b18-16d54d594be2\",\r\n        \"XpertRole\": \"\",\r\n        \"isQualys\": 0,\r\n        \"Name\": \"2666c891-36a8-4380-9881-2cf1858fdcdc\",\r\n        \"S3\": \"Arcsight-TBD\",\r\n        \"S1\": \"Other\",\r\n        \"S2\": \"ArcSight-TBD\",\r\n        \"OU\": \"\",\r\n        \"S4\": \"PHX_WINC_AZWDGGENEVA001_FWDSEC\",\r\n        \"S0\": \"Unknown\"\r\n      }\r\n    }\r\n  },\r\n  \"Computer\": {\r\n    \"GenevaMetaData\": {\r\n      \"RoleInstanceName\": \"WorkerRoleApplication_IN_1\",\r\n      \"AzureIdentity\": \"2666c891-36a8-4380-9881-2cf1858fdcdc\",\r\n      \"DeploymentId\": \"Test\",\r\n      \"Tenant\": \"d4mcontactsupportrelay\"\r\n    }\r\n  },\r\n  \"SrcIp\": {\r\n    \"Anomali\": [\r\n      {\r\n        \"tags\": [\r\n          \"Brute Force\", \"Dan\", \"Russell\", \"Jose\"\r\n        ],\r\n        \"source\": \"juunde@microsoft.com\",\r\n        \"threat_type\": \"brute\",\r\n        \"id\": 1000086585,\r\n        \"confidence\": 99,\r\n        \"rdns\": null,\r\n        \"asn\": \"18403\",\r\n        \"org\": \"FPT Telecom Company\",\r\n        \"update_id\": 170561,\r\n        \"severity\": \"very-high\"\r\n      }\r\n    ]\r\n  }\r\n}";

            // The JSON above deserialized into a dynamic for detection purposes.
            var expando = JsonConvert.DeserializeObject<ExpandoObject>(eventJsonString);

            // Detection returns a value with Jose being the case-insensitive return matching value
            string query = "database(\"event\").win |extend AnomaliTags = toupper(tostring(SrcIp.Anomali[0].tags)) | where AnomaliTags has \"JOSE\"";
            var detectionTrue = RunAtomicQueryWithKqlNode(expando, query);
            Assert.IsTrue(detectionTrue.Any());

            // Detection doesn't return Jeff doesn't exist in the list.
            string queryFalse = "database(\"event\").win | extend AnomaliTags = tostring(SrcIp.Anomali[0].tags) | where AnomaliTags has \"Jeff\"";
            var detectionFalse = RunAtomicQueryWithKqlNode(expando, queryFalse);
            Assert.IsTrue(!detectionFalse.Any());
        }
    }
}