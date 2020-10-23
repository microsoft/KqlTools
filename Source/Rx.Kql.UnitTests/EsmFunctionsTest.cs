// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

using System;

namespace Rx.Kql.UnitTests
{
    using System;
    using System;
    using System.Dynamic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EsmFunctionsTest : TestBase
    {
        [TestMethod]
        public void InSubnet()
        {
            dynamic evt = new ExpandoObject();
            evt.address = "10.15.4.105";
            TestWhere(evt, "InSubnet(address,\"10.15.4.0/24\")", true);
            TestWhere(evt, "InSubnet(address,\"10.15.4.0/8\")", true);
            TestWhere(evt, "InSubnet(address,\"10.15.4.0/16\")", true);
            TestWhere(evt, "InSubnet(address,\"10.15.5.0/24\")", false);
        }

        [TestMethod]
        public void IpAddressInRange()
        {
            dynamic evt = new ExpandoObject();
            evt.address = "10.15.4.105";
            TestWhere(evt, "IpAddressInRange(address,\"10.15.4.100\", \"10.15.4.110\")", true);
            TestWhere(evt, "IpAddressInRange(address,\"10.15.4.105\", \"10.15.4.106\")", true);
            TestWhere(evt, "IpAddressInRange(address,\"10.15.3.0\", \"10.15.5.0\")", true);

            TestWhere(evt, "IpAddressInRange(address,\"10.15.4.110\", \"10.15.4.120\")", false);
            TestWhere(evt, "IpAddressInRange(address,\"10.15.5.0\", \"10.15.6.0\")", false);
        }

        [TestMethod]
        public void AgoTest()
        {
            dynamic evt = new ExpandoObject();
            DateTime dt = DateTime.UtcNow.AddMinutes(-5);
            evt.nowMinus5Minutes = dt;
            dt = DateTime.UtcNow.AddDays(-2);
            evt.nowMinusTwoDays = dt;

            TestWhere(evt, "nowMinus5Minutes > ago(2d)", true);
            TestWhere(evt, "nowMinusTwoDays > ago(1d)", false);
        }

        [TestMethod]
        public void Hash_Sha256()
        {
            dynamic evt = new ExpandoObject();
            evt.Computer1 = "AMS05ISBRP014.gme.gbl";
            evt.Computer2 = "CH1PHY140060405.phx.gbl";

            TestWhere(evt, "hash_sha256(Computer1) == \"e0645623400806a3a9e1054c5ed09297a76d5ddfaf00c76967339f6c47d34809\"", true);
            TestWhere(evt, "hash_sha256(Computer2) == \"4fba4d37b65f7882f8f7af7cb71017b60efc6630d6c12b40724b2a0de1bace4a\"", true);
            TestWhere(evt, "hash_sha256(Computer2) == \"WrongHashValue\"", false);
            TestWhere(evt, "hash_sha256(Computer2) != \"WrongHashValue\"", true);
        }

        [TestMethod]
        public void Regex_MatchesTrue()
        {
            dynamic evt = new ExpandoObject();
            evt.ProcPath = @"c:\temp\CabArc\SomePositiveFile.exe";
            evt.Computer2 = "CH1PHY140060405.phx.gbl";

            TestWhere(evt, "ProcPath matches regex @\"(?i)^.:\\\\Temp\\\\CabArc\"", true);
        }

        [TestMethod]
        public void Regex_MatchesFalse()
        {
            dynamic evt = new ExpandoObject();
            evt.ProcPath = @"HelloWorld";
            evt.Computer2 = "CH1PHY140060405.phx.gbl";

            TestWhere(evt, "ProcPath matches regex @\"(?i)^.:\\\\Temp\\\\CabArc\"", false);
        }
    }
}