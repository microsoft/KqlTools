using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rx.Kql.UnitTest;

namespace Rx.Kql.UnitTests.OperatorsTest
{
    [TestClass]
    public class LetOperatorTests : TestBase
    {
        [TestMethod]
        public void SimpleLetStatementTest()
        {
            //dynamic evt = new ExpandoObject();

            //evt.twelveHoursAgo = DateTime.UtcNow.AddHours(-12);
            //evt.EventId = 4688;
            //string query = "let start = ago(1h); let period = 1h; " +

            //               "SecurityLog | where TimeCreated > start and TimeCreated<start + period " +
            //               "| summarize count() by EventId, bin(TimeCreated, 1h)";
            //var z = RunAtomicQuery(evt, query);

            //query =
            //    "Table | extend x =  split(replace(stringtoreplace,replacestring,inputstring), splitstring) | extend ip = x[0], port = x[1] | project ip, port";
            //var zz = RunAtomicQueryWithKqlNode(evt, query);
            //var yy = zz.ToArray();
            //var aa = (IDictionary<string, object>) yy[0];

            //Assert.AreEqual(aa["ip"], "10.83.80.127");
            //Assert.AreEqual(aa["port"], "54544");
        }
    }
}
