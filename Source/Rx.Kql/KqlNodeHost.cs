// /********************************************************
// *                                                       *
// *   Copyright (C) Microsoft. All rights reserved.       *
// *   Licensed under the MIT license.                     *
// *                                                       *
// ********************************************************/

namespace System.Reactive.Kql
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Kql.CustomTypes;

    /// <summary>
    ///     Class <c>KqlNodeHost</c>  An instantiable class to host a list of KqlNodeHub objects providing a single managed object for grouped detection
    /// execution supporting routed output to Action delegates per hub.  Users can create their own Host enumerable from KqlNodeHub objects, or rely upon this simple implementation.
    /// </summary>
    public class KqlNodeHost : IDisposable
    {
        public ConcurrentDictionary<string, KqlNodeHub> KqlNodeHubs { get; set; }

        public KqlNodeHost()
        {
            KqlNodeHubs = new ConcurrentDictionary<string, KqlNodeHub>();
        }

        /// <summary>
        ///     Create a KqlNodeHub from existing *.csl files. Csl files contain one of more KQL queries as strings.
        ///     The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence,
        ///     observableInput.
        /// </summary>
        /// <param name="observableInput">
        ///     IObservable<IDictionary
        ///     <string, object>> - the data that the KQL queries will run against.
        /// </param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">string - the name of the subscription.</param>
        /// <param name="fileList">string[] - one or more paths to the *.csl files.</param>
        /// <returns>a boolean value containing the operations success.</returns>
        public bool AddFromFiles(
            IObservable<IDictionary<string, object>> observableInput,
            Action<KqlOutput> delegateOutput,
            string observableName,
            params string[] fileList)
        {
            var kqlNodeHub = new KqlNodeHub(delegateOutput);

            foreach (string f in fileList)
            {
                kqlNodeHub._node.AddCslFile(f);
            }

            kqlNodeHub.AddInput(observableName, observableInput);

            return KqlNodeHubs.TryAdd(observableName, kqlNodeHub);
        }

        /// <summary>
        ///     Create a KqlNodeHub from an existing KQL query as a string. The string must contain only one KQL query.
        ///     The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence,
        ///     observableInput.
        /// </summary>
        /// <param name="observableInput">
        ///     IObservable<IDictionary
        ///     <string, object>> - the data that the KQL queries will run against.
        /// </param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">the observable input stream name</param>
        /// <param name="kqlQuery">string - the KQL query as a string.</param>
        /// <param name="comment"></param>
        /// <param name="description"></param>
        /// <returns>a boolean value containing the operations success.</returns>
        public bool AddFromKqlQuery(
            IObservable<IDictionary<string, object>> observableInput,
            Action<KqlOutput> delegateOutput,
            string observableName,
            string kqlQuery, 
            string comment = null,
            string description = null
            )
        {
            var kqlNodeHub = new KqlNodeHub(delegateOutput);

            kqlNodeHub._node.AddKqlQuery(new CslParagraph
            {
                Query = kqlQuery,
                Description = description,
                Comment = comment
            });

            kqlNodeHub.AddInput(observableName, observableInput);

            return KqlNodeHubs.TryAdd(observableName, kqlNodeHub);
        }

        /// <summary>
        ///     Create a KqlNodeHub from an existing KQL query as a string. The string must contain only one KQL query.
        ///     The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence,
        ///     observableInput.
        /// </summary>
        /// <param name="observableInput">
        ///     IObservable<IDictionary
        ///     <string, object>> - the data that the KQL queries will run against.
        /// </param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">the observable input stream name</param>
        /// <param name="kqlQuery">KqlQuery - an instance of the KqlQuery object type</param>
        /// <returns>a boolean value containing the operations success.</returns>
        public bool AddFromKqlQuery(
            IObservable<IDictionary<string, object>> observableInput,
            Action<KqlOutput> delegateOutput,
            string observableName,
            KqlQuery kqlQuery)
        {
            var kqlNodeHub = new KqlNodeHub(delegateOutput);

            kqlNodeHub._node.AddKqlQuery(kqlQuery);

            kqlNodeHub.AddInput(observableName, observableInput);

            return KqlNodeHubs.TryAdd(observableName, kqlNodeHub);
        }

        /// <summary>
        ///     Create a KqlNodeHub from an existing KQL query as a string. The string must contain only one KQL query.
        ///     The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence,
        ///     observableInput.
        /// </summary>
        /// <param name="observableInput">
        ///     IObservable<IDictionary
        ///     <string, object>> - the data that the KQL queries will run against.
        /// </param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">the observable input stream name</param>
        /// <param name="kqlQueryList">KqlQuery - a list of instances of the KqlQuery object type</param>
        /// <returns>a boolean value containing the operations success.</returns>
        public bool AddFromKqlQueryList(
            IObservable<IDictionary<string, object>> observableInput,
            Action<KqlOutput> delegateOutput,
            string observableName,
            List<KqlQuery> kqlQueryList)
        {
            var kqlNodeHub = new KqlNodeHub(delegateOutput);

            kqlNodeHub._node.AddKqlQueryList(kqlQueryList);

            kqlNodeHub.AddInput(observableName, observableInput);

            return KqlNodeHubs.TryAdd(observableName, kqlNodeHub);
        }

        /// <summary>
        /// Removes a desired instance of the KqlNodeHub from the Hosts list.
        /// </summary>
        /// <param name="observableName"></param>
        /// <returns>a boolean value containing the operations success.</returns>
        public bool Remove(string observableName)
        {
            return KqlNodeHubs.TryRemove(observableName, out KqlNodeHub kqlNodeHub);
        }

        /// <summary>
        ///     Performs required clean up work when the KqlNodeHost is disposed of.
        /// </summary>
        public void Dispose()
        {
            KqlNodeHubs = null;
        }
    }
}