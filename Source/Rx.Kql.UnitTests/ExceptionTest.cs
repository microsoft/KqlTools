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
    using System.Reactive.Kql.ExceptionTypes;
    using System.Reactive.Subjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ExceptionTest : TestBase
    {
        //[TestMethod]
        //public void Throw()
        //{
        //    var subject = new Subject<IDictionary<string, object>>();
        //    var result = new List<IDictionary<string, object>>();
        //    Exception ex = null;

        //    subject.KustoQuery("where Throw(5)")
        //        .Subscribe(e => { result.Add(e); },
        //            (Exception e) => { ex = e; });

        //    for (int i = 0; i < 10; i++)
        //    {
        //        dynamic evt = new ExpandoObject();
        //        evt.value = i;
        //        subject.OnNext(evt);
        //    }

        //    Assert.IsNotNull(ex);
        //    Assert.AreEqual(4, result.Count);
        //    Assert.IsTrue(true);
        //}

        [TestMethod]
        public void UnknownFunction()
        {
            dynamic evt = new ExpandoObject();
            evt.address = "x";

            var subject = new Subject<IDictionary<string, object>>();
            var result = new List<IDictionary<string, object>>();

            try
            {
                subject.KustoQuery("where foo()")
                    .Subscribe(e => { result.Add(e); });

                subject.OnNext(evt);
                Assert.Fail();
            }
            catch (UnknownFunctionException e)
            {
                Assert.AreEqual(e.Message, "Unknown function: 'foo'");
            }

            Assert.AreEqual(0, result.Count);
        }
    }
}