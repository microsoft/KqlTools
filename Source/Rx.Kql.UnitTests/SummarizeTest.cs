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
    using System.Linq;
    using System.Reactive.Kql;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SummarizeTest : TestBase
    {
        [TestMethod]
        public void SummarizeOperatorTestWithMaxListCount_MakeSet()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = TestList(
                "| summarize r = make_set(Symbol,5) by bin(Time, 1m)",
                qt => from q in qt
                      group q by new
                      {
                          q.Time.Minute
                      }
                    into window
                      select new SummaryList
                      {
                          Time = window.Key.Minute,
                          Result = MakeList(window.Select(e => e.Symbol).ToList())
                      }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTestWithoutMaxListCount_MakeSet()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = TestList(
                "| summarize r = make_set(Symbol) by bin(Time, 1m)",
                qt => from q in qt
                      group q by new
                      {
                          q.Time.Minute
                      }
                    into window
                      select new SummaryList
                      {
                          Time = window.Key.Minute,
                          Result = MakeList(window.Select(e => e.Symbol).ToList())
                      }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTestWithMaxListCount_MakeList()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = TestList(
                "| summarize r = make_list(Symbol,5) by bin(Time, 1m)",
                qt => from q in qt
                      group q by new
                      {
                          q.Time.Minute
                      }
                    into window
                      select new SummaryList
                      {
                          Time = window.Key.Minute,
                          Result = MakeList(window.Select(e=>e.Symbol).ToList())
                      }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTestWithoutMaxListCount_MakeList()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = TestList(
                "| summarize r = make_list(Symbol) by bin(Time, 1m)",
                qt => from q in qt
                      group q by new
                      {
                          q.Time.Minute
                      }
                    into window
                      select new SummaryList
                      {
                          Time = window.Key.Minute,
                          Result = MakeList(window.Select(e => e.Symbol).ToList())
                      }, quotes);

            Assert.IsTrue(result);
        }
        private static string MakeList(List<string> strList)
        {
            StringBuilder sbList = new StringBuilder();
            sbList.AppendLine("[");
            for (int i = 0; i < strList.Count; i++)
            {
                sbList.AppendLine(strList[i]);
            }
            sbList.AppendLine("]");

            return sbList.ToString();
        }

        [TestMethod]
        public void SummarizeOperatorTest_Count()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = Test(
                "| summarize r = count() by Symbol, bin(Time, 1m)",
                qt => from q in qt
                    group q by new
                    {
                        q.Symbol,
                        q.Time.Minute
                    }
                    into window
                    select new Summary
                    {
                        Symbol = window.Key.Symbol,
                        Time = window.Key.Minute,
                        Result = window.Count()
                    }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTest_Sum()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = Test(
                "| summarize r = sum(Price) by Symbol, bin(Time, 1m)",
                qt => from q in qt
                    group q by new
                    {
                        q.Symbol,
                        q.Time.Minute
                    }
                    into window
                    select new Summary
                    {
                        Symbol = window.Key.Symbol,
                        Time = window.Key.Minute,
                        Result = window.Sum(e => e.Price)
                    }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTest_Min()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = Test(
                "| summarize r = min(Price) by Symbol, bin(Time, 1m)",
                qt => from q in qt
                    group q by new
                    {
                        q.Symbol,
                        q.Time.Minute
                    }
                    into window
                    select new Summary
                    {
                        Symbol = window.Key.Symbol,
                        Time = window.Key.Minute,
                        Result = window.Min(e => e.Price)
                    }, quotes);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SummarizeOperatorTest_Max()
        {
            var data = new TestData(5);
            StockQuote[] quotes = data.GetQuotes(1, 300).ToArray();

            bool result = Test(
                "| summarize r = max(Price) by Symbol, bin(Time, 1m)",
                qt => from q in qt
                    group q by new
                    {
                        q.Symbol,
                        q.Time.Minute
                    }
                    into window
                    select new Summary
                    {
                        Symbol = window.Key.Symbol,
                        Time = window.Key.Minute,
                        Result = window.Max(e => e.Price)
                    }, quotes);

            Assert.IsTrue(result);
        }

        private static bool TestList(
            string kql,
            Func<IEnumerable<StockQuote>,
                IEnumerable<SummaryList>> linq,
            StockQuote[] quotes)
        {
            //KqlNode node = new KqlNode();
            //node.AddKqlQuery(new KqlQuery
            //{
            //    Comment = "Blank Comment",
            //    Query = kql
            //});

            //Console.WriteLine(kql);
            //Console.WriteLine();

            var linqResult = linq(quotes)
                    .Select(a => string.Format("{0} {1} {2}", a.Time, a.Symbol, a.Result))
                    .ToArray();

            List<string> kqlResult = new List<string>();

            var obs = quotes.ToObservable()
                .ToDynamic(e => e);

            KqlNode kqlNode = new KqlNode();
            List<KqlQuery> kustoQueryUserInput = new List<KqlQuery>
            {
                new KqlQuery
                {
                    Comment = "Blank Comment",
                    Query = kql.Trim()
                }
            };
            kqlNode.AddKqlQueryList(kustoQueryUserInput, true);

            ManualResetEvent completedEvent = new ManualResetEvent(false);

            if (kql.Contains("makelist") || 
                kql.Contains("make_list") ||
                kql.Contains("makeset") ||
                kql.Contains("make_set")
                )
            {
                kqlNode.Subscribe(evt =>
                {
                    kqlResult.Add(string.Format("{0} {1}",
                    ((DateTime)evt.Output["Time"]),
                                  MakeList((List<string>)evt.Output["r"])));
                },
                () =>
                {
                    completedEvent.Set();
                });
            }
            else
            {
                kqlNode.Subscribe(evt =>
                {
                    kqlResult.Add(string.Format("{0} {1} {2}",
                        ((DateTime)evt.Output["Time"]),
                        evt.Output["Symbol"],
                        evt.Output["r"]));
                },
                () =>
                {
                    completedEvent.Set();
                });
            }
            using (obs.Subscribe(kqlNode))
            {
                completedEvent.WaitOne();
                kqlNode.OnCompleted();
            }

            Console.WriteLine("  LINQ            Rx.KQL");

            for (int i = 0; i < linqResult.Length; i++)
            {
                Console.WriteLine("{0}\t{1}", linqResult[i], kqlResult[i]);
            }

            return linqResult.Length == kqlResult.Count;
        }

        private static bool Test(
            string kql,
            Func<IEnumerable<StockQuote>, IEnumerable<Summary>> linq,
            StockQuote[] quotes)
        {
            KqlNode node = new KqlNode();
            node.AddKqlQuery(new KqlQuery
            {
                Comment = "Blank Comment",
                Query = kql
            });

            Console.WriteLine(kql);
            Console.WriteLine();

            var linqResult = linq(quotes)
                    .Select(a => string.Format("{0} {1} {2}", a.Time, a.Symbol, a.Result))
                    .ToArray();

            List<string> kqlResult = new List<string>();

            var obs = quotes.ToObservable()
                .ToDynamic(e => e);

            KqlNode kqlNode = new KqlNode();
            List<KqlQuery> kustoQueryUserInput = new List<KqlQuery>
            {
                new KqlQuery
                {
                    Comment = "Blank Comment",
                    Query = kql.Trim()
                }
            };

            kqlNode.AddKqlQueryList(kustoQueryUserInput, true);

            ManualResetEvent completedEvent = new ManualResetEvent(false);

                kqlNode.Subscribe(evt =>
                {
                    kqlResult.Add(string.Format("{0} {1} {2}",
                        ((DateTime)evt.Output["Time"]),
                        evt.Output["Symbol"],
                        evt.Output["r"]));
                },
                    () =>
                    {
                        completedEvent.Set();
                    });
            
            using (obs.Subscribe(kqlNode))
            {
                completedEvent.WaitOne();
                kqlNode.OnCompleted();
            }

            Console.WriteLine("  LINQ            Rx.KQL");

            for (int i = 0; i < linqResult.Length; i++)
            {
                Console.WriteLine("{0}\t{1}", linqResult[i], kqlResult[i]);
            }

            return linqResult.Length == kqlResult.Count;
        }
    }

    internal class StockQuote
    {
        public DateTime Time { get; set; }
        public string Symbol { get; set; }
        public int Price { get; set; }
    }

    internal class Summary
    {
        public int Time { get; set; }
        public string Symbol { get; set; }
        public long Result { get; set; }
    }

    internal class SummaryList
    {
        public int Time { get; set; }
        public string Symbol { get; set; }
        public string Result { get; set; }
    }

    internal class TestData
    {
        static readonly string[] _symbols = new string[]
        {
            "MSFT",
            "AMZN",
            "GOOG"
        };

        DateTime _start = new DateTime(2018, 1, 1);
        readonly Random _r;

        public TestData(int seed)
        {
            _r = new Random(seed);
        }

        internal IEnumerable<StockQuote> GetQuotes(int intervalSeconds, int count)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (string s in _symbols)
                {
                    var quote = new StockQuote
                    {
                        Time = _start.AddSeconds(i * intervalSeconds),
                        Symbol = s,
                        Price = _r.Next(100)
                    };

                    yield return quote;
                }
            }
        }
    }
}