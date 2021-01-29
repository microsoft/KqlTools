// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Reactive.Kql.CustomTypes;
    using System.Reactive.Kql.EventTypes;
    using System.Reactive.Subjects;

    /// <summary>
    ///     Class <c>KqlNode</c>  Helper class for managing multiple standing queries defined in KQL
    ///     It also serves as environment which is extensible with globally known lists and functions.
    /// </summary>
    public class KqlNode : IObserver<IDictionary<string, object>>, IObservable<KqlOutput>, IDisposable
    {
        /// <summary>
        ///     (Read only) Specifies the type of output the KqlNode should produce.
        /// </summary>
        public readonly Subject<KqlOutput> Output = new Subject<KqlOutput>();

        /// <summary>
        ///     The List of all KQL queries (as KqlQuery instances) currently loaded in the KqlNode.
        /// </summary>
        /// <see cref="KqlQuery" />
        public List<KqlQuery> KqlQueryList { get; private set; }

        /// <summary>
        ///     The List of all KQL queries currently loaded in the KqlNode that have failed execution.
        /// </summary>
        /// <see cref="KqlQuery" />
        public List<KqlQuery> FailedKqlQueryList { get; private set; }

        private List<IDisposable> SubscriptionReferenceDisposables { get; set; }

        /// <summary>
        ///     Enables / Disables whether failed query events are raised by the KqlNode.
        ///     Default is false, do not raise these events.
        /// </summary>
        public bool EnableFailedKqlQueryEvents { get; set; } = false;

        /// <summary>
        ///     Enables / Disables whether to dispose of a KQL query if it raises an exception.
        ///     Default is false, do not dispose of a KQL query if it raises an exception.
        /// </summary>
        public bool DisposeKqlQueryOnException { get; set; } = false;

        /// <summary>
        ///     Event will be raised on completion of a KqlQuery.
        /// </summary>
        public event EventHandler<KqlQueryCompletedEventArgs> KqlKqlQueryCompleted;

        /// <summary>
        ///     Event will be raised on a failure of a KqlQuery, if EnableFailedKqlQueryEvents is set to 'true'.
        /// </summary>
        public event EventHandler<KqlQueryFailedEventArgs> KqlKqlQueryFailed;

        /// <summary>
        ///     Creates an new KqlNode instance and initializes the KqlQueryList, FailedKqlQueryList and
        ///     SubscriptionReferenceDisposables.
        /// </summary>
        public KqlNode()
        {
            InitializeKqlQueryLists();
        }

        /// <summary>
        ///     Adds all the KQL queries defined in the specified *.csl file to the KqlNode.
        /// </summary>
        /// <param name="filepath">string - the path to the *.cl file containing one or more KQL queries.</param>
        /// <param name="stopKqlQuerys">
        ///     Boolean - If set to 'true', stop and remove all currently-running queries before adding new
        ///     queries.
        /// </param>
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

        /// <summary>
        ///     Adds the KQL query defined in the CslParagraph object, to the KqlNode.
        ///     If the KQL query cannot be parsed or added, adds the query to the FailedKqlQueryList.
        /// </summary>
        /// <param name="para">CslParagraph -- the CslParagraph instance that defined the KQL query and associated metadata.</param>
        /// <see cref="CslParagraph" />
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

        /// <summary>
        ///     Attempts to add the KQL query defined in the CslParagraph object, to the KqlNode.
        ///     If the KQL query cannot be parsed or added, adds the query to the FailedKqlQueryList and returns the input
        ///     Exception.
        /// </summary>
        /// <param name="para">CslParagraph -- the CslParagraph instance that defined the KQL query and associated metadata.</param>
        /// <param name="exception">Exception - an instance of System.Exception.</param>
        /// <returns>Boolean - 'true' is the operation succeed; otherwise 'false'.</returns>
        /// <see cref="CslParagraph" />
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

        /// <summary>
        ///     Attempts to add the KQL query defined in the KqlQiery object, to the KqlNode.
        ///     If the KQL query cannot be parsed or added, adds the query to the FailedKqlQueryList and returns the input
        ///     Exception.
        /// </summary>
        /// <param name="para">KqlQuery -- the KqlQuery instance that defined the KQL query and associated metadata.</param>
        /// <param name="exception">Exception - an instance of System.Exception.</param>
        /// <returns>Boolean - 'true' is the operation succeed; otherwise 'false'.</returns>
        /// <see cref="KqlQuery" />
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
        ///     Adds one or more KQL functions (from the KqlNode) directly from definitions in an input string.
        /// </summary>
        /// <param name="kqlFunctionQuery">
        ///     string - a string with one or more string representations of a CslParagraph, separated
        ///     by live feeds.
        /// </param>
        public void AddKqlFunction(string kqlFunctionQuery)
        {
            // Load any functions for use across the node
            GlobalFunctions.ReadFunctionsFromQuery(kqlFunctionQuery);
        }

        /// <summary>
        ///     Removes one or more KQL functions (from the KqlNode) directly from definitions in an input string.
        /// </summary>
        /// <param name="kqlFunctionQuery">
        ///     string - a string with one or more string representations of a CslParagraph, separated
        ///     by live feeds.
        /// </param>
        public void RemoveKqlFunction(string kqlFunctionName)
        {
            // Load any functions for use across the node
            GlobalFunctions.Remove(kqlFunctionName);
        }

        /// <summary>
        ///     Retrieves a KQL function by its function name, from the current list of KQL functions in the KqlNode.
        /// </summary>
        /// <param name="kqlFunctionName">string - the name of the function. </param>
        /// <see cref="CslFunction" />
        public CslFunction GetKqlFunction(string kqlFunctionName)
        {
            // Retrieve requested function the node
            return GlobalFunctions.GetFunction(kqlFunctionName);
        }

        /// <summary>
        ///     Adds a single KqlQuery to the KqlNode from a KqlQuery instance.
        /// </summary>
        /// <param name="kqlQuery">The KqlQuery object</param>
        /// <param name="stopKqlQueries">
        ///     Optionally clear out currently executing KQL queries if set to 'true', or only add new
        ///     ones if set to 'false' (default).
        /// </param>
        /// <see cref="KqlQuery" />
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
        ///     Adds a list of KqlQuery objects to the KqlNode.
        /// </summary>
        /// <param name="kqlQuery">List<KqlQuery> - The list of KqlQuery objects</param>
        /// <param name="stopKqlQueries">
        ///     Boolean - Optionally clear out currently executing KqlQueries if set to 'true', or only
        ///     add new ones if set to 'false' (default).
        /// </param>
        /// <see cref="KqlQuery" />
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
        ///     [Left in for legacy purposes only]
        ///     Use AddKqlQuery instead.
        /// </summary>
        /// <param name="kqlQuery">KqlQuery - The KqlQuery object</param>
        /// <param name="stopKqlQueries">
        ///     Boolean - Optionally clear out currently executing KqlQueries if set to 'true', or only
        ///     add new ones if set to 'false' (default).
        /// </param>
        /// <see cref="KqlQuery" />
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

        /// <summary>
        ///     Makes each KQL query in the KqlNode to execute on the next set of data.
        /// </summary>
        /// <param name="value">IDictionary<string, object> - the set of data for the KQL queries to operate on.</param>
        public void OnNext(IDictionary<string, object> value)
        {
            var failedKqlQuerys = new List<KqlQueryFailure>();
            Stopwatch kqlQueryStopwatch = new Stopwatch();
            var queries = KqlQueryList; //make local copy, for concurrency
            foreach (var d in queries)
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
        ///     When a KQL query fails execution, raise this event to the hosting process.
        /// </summary>
        /// <param name="args">KqlQueryFailedEventArgs - the arguments for the KQL query failure.</param>
        private void OnKqlQueryFailed(KqlQueryFailedEventArgs args)
        {
            KqlKqlQueryFailed?.Invoke(this, args);
        }

        /// <summary>
        ///     When a KQL query completes, raise the event to the hosting process.
        /// </summary>
        /// <param name="args">KqlQueryCompletedEventArgs - the arguments for the KQL query completion.</param>
        private void OnKqlQueryCompleted(KqlQueryCompletedEventArgs args)
        {
            KqlKqlQueryCompleted?.Invoke(this, args);
        }

        /// <summary>
        ///     Processes required work when an exception is passed to it.
        /// </summary>
        /// <param name="error">System.Exception - the input exception.</param>
        public void OnError(Exception error)
        {
            // Pass the correct output downstream
            Output.OnError(error);
        }

        /// <summary>
        ///     Processes required clean up work, if any, when the KqlNode completes all queries.
        /// </summary>
        public void OnCompleted()
        {
            var queries = KqlQueryList; //make local copy, for concurrency
            foreach (var d in queries)
            {
                d.Input.OnCompleted();
            }

            Output.OnCompleted();
        }

        /// <summary>
        ///     Initializes the KqlQueryList, FailedKqlQueryList and SubscriptionReferenceDisposables.
        ///     This is equivalent to resetting the KqlNode instance. All running queries will immediately stop.
        /// </summary>
        public void InitializeKqlQueryLists()
        {
            // Initialize KqlQuery lists
            KqlQueryList = new List<KqlQuery>();
            FailedKqlQueryList = new List<KqlQuery>();
            SubscriptionReferenceDisposables = new List<IDisposable>();
        }

        /// <summary>
        ///     Subscribes an observer to the Subject, in this case Output.
        /// </summary>
        /// <param name="observer">IObserver<KqlOutput> - Observer to subscribe to the subject.</param>
        /// <returns>IDisposable - Disposable object that can be used to unsubscribe the observer from the subject.</returns>
        public IDisposable Subscribe(IObserver<KqlOutput> observer)
        {
            return Output.Subscribe(observer);
        }

        /// <summary>
        ///     Performs required clean up work when the KqlNode is disposed of.
        /// </summary>
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

    /// <summary>
    ///     Class <c>KqlQuery</c> container for a KQL query and associated metadata.
    /// </summary>
    public class KqlQuery : IDisposable
    {
        /// <summary>
        ///     The KqlNode instance that this KqlQuery instance is associated with.
        /// </summary>
        /// <see cref="KqlNode" />
        public KqlNode Node { get; set; }

        /// <summary>
        ///     (Read only) Specifies the type of input the KqlNode should consume.
        /// </summary>
        public Subject<IDictionary<string, object>> Input { get; } = new Subject<IDictionary<string, object>>();

        /// <summary>
        ///     A comment describing the KQL query.
        /// </summary>
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

        /// <summary>
        /// Evaluate a single query at a time
        /// </summary>
        /// <param name="dict">The IDictionary of the data to be evaluated with this KqlQuery</param>
        /// <returns></returns>
        public KqlOutput Evaluate(IDictionary<string, object> dict)
        {
            KqlOutput result = null;
            Stopwatch evaluateStopwatch = Stopwatch.StartNew();

            // Pass over the table name, at least one pipe char [|] is required.
            Input.KustoQuery(Query.Substring(Query.IndexOf('|') + 1).Trim())
                .Subscribe(e =>
                {
                    result = new KqlOutput
                    {
                        Output = e,
                        Comment = Comment,
                        Query = Query,
                        KqlQuery = this
                    };
                });


            // Evaluate the query pipeline
            Input.OnNext(dict);
            this.EvaluationCount++;
            this.EvaluationTimeSpan += evaluateStopwatch.Elapsed;

            return result;
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
        ///     KqlQuery Efficiency in AverageMicroseconds
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