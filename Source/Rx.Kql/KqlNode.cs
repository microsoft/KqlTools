// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Kql.CustomTypes;
    using System.Reactive.Kql.EventTypes;
    using System.Reactive.Subjects;

    /// <summary>
    ///     Helper class for managing multiple standing queries defined in KQL
    ///     It also serves as environment which is extensible with globally known
    ///     lists and functions
    /// </summary>
    public class KqlNode : IObserver<IDictionary<string, object>>, IObservable<KqlOutput>, IDisposable
    {
        public readonly Subject<KqlOutput> Output = new Subject<KqlOutput>();

        public List<KqlQuery> KqlQueryList { get; private set; }

        public List<KqlQuery> FailedKqlQueryList { get; private set; }

        private List<IDisposable> SubscriptionReferenceDisposables { get; set; }

        public bool EnableFailedKqlQueryEvents { get; set; } = false;

        public bool DisposeKqlQueryOnException { get; set; } = false;

        /// <summary>
        /// Event will be raised on completion of a KqlQuery.
        /// </summary>
        public event EventHandler<KqlQueryCompletedEventArgs> KqlKqlQueryCompleted;

        /// <summary>
        /// Event will be raised on a failure of a KqlQuery.
        /// </summary>
        public event EventHandler<KqlQueryFailedEventArgs> KqlKqlQueryFailed;

        public KqlNode()
        {
            InitializeKqlQueryLists();
        }

        public void AddCslFile(string filepath, bool stopKqlQuerys = false)
        {
            // Optionally clear out currently executing KqlQuerys, or only add new ones
            if (stopKqlQuerys)
            {
                InitializeKqlQueryLists();
            }

            foreach (var para in CslParser.ReadFile(filepath))
            {
                this.AddKqlQuery(para);
            }
        }

        public void AddKqlQuery(CslParagraph para)
        {
            try
            {
                // Load any functions for use across the node
                GlobalFunctions.ReadFunctionsFromQuery(para.Query);

                // Create the KqlQuery info, and hold reference to the object to prevent GC from releasing.
                var newKqlQuery = new KqlQuery(this, para.Comment, para.Query)
                {
                    IsValid = true,
                    Message = "Success",
                    KqlQueryEngagedDateTime = DateTime.UtcNow
                };

                KqlQueryList.Add(newKqlQuery);
                SubscriptionReferenceDisposables.Add(newKqlQuery);
            }
            catch (Exception ex)
            {
                FailedKqlQueryList.Add(new KqlQuery
                {
                    Node = this,
                    Comment = para.Comment,
                    Query = para.Query,
                    IsValid = false,
                    Message = ex.Message,
                    FailureReason = ex,
                });
            }
        }

        public bool TryAddKqlQuery(CslParagraph para, out Exception exception)
        {
            bool result = true;
            exception = null;

            this.AddKqlQuery(para);

            exception = this.FailedKqlQueryList
                    .Where(a => string.Equals(a.Query, para.Query, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.FailureReason)
                    .FirstOrDefault();

            result = exception == null;

            return result;
        }

        public bool TryAddKqlQuery(KqlQuery kqlQuery, out Exception exception)
        {
            bool result = true;
            exception = null;

            this.AddKqlQuery(kqlQuery);

            exception = this.FailedKqlQueryList
                    .Where(a => string.Equals(a.Query, kqlQuery.Query, StringComparison.OrdinalIgnoreCase))
                    .Select(a => a.FailureReason)
                    .FirstOrDefault();

            result = exception == null;

            return result;
        }

        /// <summary>
        /// Add a Kql function directly from it's definition
        /// </summary>
        /// <param name="kqlFunctionQuery"></param>
        public void AddKqlFunction(string kqlFunctionQuery)
        {
            // Load any functions for use across the node
            GlobalFunctions.ReadFunctionsFromQuery(kqlFunctionQuery);
        }

        /// <summary>
        /// Add a Kql function directly from it's definition
        /// </summary>
        /// <param name="kqlFunctionQuery"></param>
        public void RemoveKqlFunction(string kqlFunctionName)
        {
            // Load any functions for use across the node
            GlobalFunctions.Remove(kqlFunctionName);
        }

        /// <summary>
        /// Retrieve a Kql function directly from it's definition
        /// </summary>
        /// <param name="kqlFunctionName"></param>
        public CslFunction GetKqlFunction(string kqlFunctionName)
        {
            // Retrieve requested function the node
            return GlobalFunctions.GetFunction(kqlFunctionName);
        }

        /// <summary>
        /// Add a single KqlQuery to the KqlNode.
        /// </summary>
        /// <param name="kqlQuery">The KqlQuery object</param>
        /// <param name="stopKqlQueries">Optionally clear out currently executing KqlQueries, or only add new ones</param>
        public void AddKqlQuery(KqlQuery kqlQuery, bool stopKqlQueries = false)
        {
            try
            {
                // Optionally clear out currently executing KqlQueries, or only add new ones
                if (stopKqlQueries)
                {
                    InitializeKqlQueryLists();
                }

                // Load any functions for use across the node
                GlobalFunctions.ReadFunctionsFromQuery(kqlQuery.Query);

                // Create the KqlQuery info, and hold reference to the object to prevent GC from releasing.
                var newKqlQuery = new KqlQuery(this, kqlQuery.Comment, kqlQuery.Query)
                {
                    IsValid = true,
                    Message = "Success",
                    KqlQueryEngagedDateTime = DateTime.UtcNow,
                    QueryGuid = kqlQuery.QueryGuid,
                    QueryName = kqlQuery.QueryName,
                    QueryDescription = kqlQuery.QueryDescription,
                    QueryId = kqlQuery.QueryId,
                };

                KqlQueryList.Add(newKqlQuery);
                SubscriptionReferenceDisposables.Add(newKqlQuery);
            }
            catch (Exception ex)
            {
                FailedKqlQueryList.Add(new KqlQuery
                {
                    Node = this,
                    Comment = kqlQuery.Comment,
                    Query = kqlQuery.Query,
                    IsValid = false,
                    Message = ex.Message,
                    QueryGuid = kqlQuery.QueryGuid,
                    QueryName = kqlQuery.QueryName,
                    QueryDescription = kqlQuery.QueryDescription,
                    QueryId = kqlQuery.QueryId,
                    FailureReason = ex,
                });
            }
        }

        /// <summary>
        /// Add a complete list of KqlQuery objects to the KqlNode.
        /// </summary>
        /// <param name="kqlQuery">The list of KqlQuery objects</param>
        /// <param name="stopKqlQueries">Optionally clear out currently executing KqlQueries, or only add new ones</param>
        public void AddKqlQueryList(List<KqlQuery> kqlQueryList, bool stopKqlQuerys = false)
        {
            // Optionally clear out currently executing KqlQuerys, or only add new ones
            if (stopKqlQuerys)
            {
                InitializeKqlQueryLists();
            }

            foreach (var kqlQueryInfo in kqlQueryList)
            {
                AddKqlQuery(kqlQueryInfo, false);
            }
        }

        /// <summary>
        /// Left in for legacy purposes.  Usage should be on the published and supported method: AddKqlQuery
        /// </summary>
        /// <param name="kqlQuery">The KqlQuery object</param>
        /// <param name="stopKqlQueries">Optionally clear out currently executing KqlQueries, or only add new ones</param>
        public void AddKqlQueryInfo(KqlQuery kqlQuery, bool stopKqlQueries = false)
        {
            if (stopKqlQueries)
            {
                InitializeKqlQueryLists();
            }

            this.AddKqlQuery(new CslParagraph
            {
                Comment = kqlQuery.Comment,
                Query = kqlQuery.Query
            });
        }

        public void OnNext(IDictionary<string, object> value)
        {
            var failedKqlQuerys = new List<KqlQueryFailure>();
            Stopwatch kqlQueryStopwatch = new Stopwatch();
            foreach (var d in KqlQueryList)
            {
                try
                {
                    // Restart the stopwatch on each KqlQuery
                    kqlQueryStopwatch.Restart();
                    d.Input.OnNext(value);
                    kqlQueryStopwatch.Stop();
                    d.EvaluationCount++;
                    d.EvaluationTimeSpan += kqlQueryStopwatch.Elapsed;
                }
                catch (Exception ex)
                {
                    d.FailureReason = ex;

                    // If the node is set to dispose of queries on execution exceptions.
                    if (DisposeKqlQueryOnException)
                    {
                        d.Dispose();
                    }
                    else
                    {
                        d.EnableSubscription(d.Query);
                    }

                    // Always raise the event if enabled
                    failedKqlQuerys.Add(new KqlQueryFailure
                    {
                        KqlQuery = d,
                        Exception = ex,
                        EventDictionary = value
                    });
                }
            }

            foreach (var failure in failedKqlQuerys)
            {
                // Potentially causing KqlQuery removal
                // KqlQuerys.Remove(d);
                if (EnableFailedKqlQueryEvents)
                {
                    OnKqlQueryFailed(new KqlQueryFailedEventArgs
                    {
                        Comment = failure.KqlQuery.Comment,
                        Exception = failure.Exception,
                        FailureDateTime = DateTime.UtcNow,
                        Message = failure.Exception.Message,
                        Query = failure.KqlQuery.Query,
                        EventDictionary = failure.EventDictionary
                    });
                }
            }
        }

        /// <summary>
        /// When a KqlQuery Fails, raise it to the hosting process
        /// </summary>
        /// <param name="args">the arguments for the KqlQuery failure</param>
        private void OnKqlQueryFailed(KqlQueryFailedEventArgs args)
        {
            KqlKqlQueryFailed?.Invoke(this, args);
        }

        /// <summary>
        /// When a KqlQuery Completes, raise it to the hosting process
        /// </summary>
        /// <param name="args">the arguments for the KqlQuery completion</param>
        private void OnKqlQueryCompleted(KqlQueryCompletedEventArgs args)
        {
            KqlKqlQueryCompleted?.Invoke(this, args);
        }

        public void OnError(Exception error)
        {
            // Pass the correct output downstream
            Output.OnError(error);
        }

        public void OnCompleted()
        {
            foreach (var d in KqlQueryList)
            {
                d.Input.OnCompleted();
            }

            Output.OnCompleted();
        }

        public void InitializeKqlQueryLists()
        {
            // Initialize KqlQuery lists
            KqlQueryList = new List<KqlQuery>();
            FailedKqlQueryList = new List<KqlQuery>();
            SubscriptionReferenceDisposables = new List<IDisposable>();
        }

        public IDisposable Subscribe(IObserver<KqlOutput> observer)
        {
            return Output.Subscribe(observer);
        }

        public void Dispose()
        {
            if (this.SubscriptionReferenceDisposables == null)
            {
                return;
            }

            foreach (var item in this.SubscriptionReferenceDisposables)
            {
                item.Dispose();
            }
        }
    }

    public class KqlQuery : IDisposable
    {
        public KqlNode Node { get; set; }

        public Subject<IDictionary<string, object>> Input { get; } = new Subject<IDictionary<string, object>>();

        public string Comment { get; set; }

        public string Query { get; set; }

        // The Subscribe returns handle that cancels the subscription, thus we need to keep the query in scope
        public IDisposable Subscription { get; set; }

        public Exception FailureReason { get; set; }

        public bool IsValid { get; set; }

        public string Message { get; set; }

        public DateTime KqlQueryEngagedDateTime { get; set; }

        public Guid QueryGuid { get; set; }

        public long QueryId { get; set; }

        public string QueryName { get; set; }

        public string QueryDescription { get; set; }

        public long EvaluationCount { get; set; } = 0;

        public TimeSpan EvaluationTimeSpan { get; set; }

        public KqlQuery()
        {
            // Empty Constructor
        }

        public KqlQuery(
            KqlNode node,
            string comment,
            string query)
        {
            Node = node;
            Comment = comment;

            EnableSubscription(query);
        }

        public void Dispose()
        {
            if (this.Subscription == null)
            {
                return;
            }

            Subscription.Dispose();
        }

        public void EnableSubscription(string query)
        {
            int pipeIndex = query.IndexOf('|');

            // The original query cannot change on the KqlQuery class, which leads to consumer confusion.
            Query = query;

            if (pipeIndex != -1) // This means there is at least one Pipe
            {
                Subscription = Input.KustoQuery(Query.Substring(pipeIndex + 1).Trim())
                    .Subscribe(d =>
                    {
                        KqlOutput a = new KqlOutput
                        {
                            Output = d,
                            Comment = Comment,
                            Query = query,
                            KqlQuery = this
                        };

                        // Push downstream to subscribers
                        Node.Output.OnNext(a);
                    });
            }
            else // If there is no pipe in the query, we are just getting everything
            {
                Subscription = Input.Subscribe(d =>
                {
                    KqlOutput a = new KqlOutput
                    {
                        Output = d,
                        Comment = Comment,
                        Query = query,
                        KqlQuery = this
                    };

                    // Push downstream to subscribers
                    Node.Output.OnNext(a);
                });
            }
        }

        /// <summary>
        /// KqlQuery Efficiency in AverageMicroseconds
        /// </summary>
        public double AverageMicroseconds
        {
            get
            {
                if (EvaluationCount > 0)
                {
                    return EvaluationTimeSpan.TotalMilliseconds * 1000 / EvaluationCount;
                }

                return 0;
            }
        }
    }
}