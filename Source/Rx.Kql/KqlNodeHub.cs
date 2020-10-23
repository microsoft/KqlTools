using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;
using System.Text;

namespace System.Reactive.Kql
{
    // Class which completely hides the usage of Reactive Extensions
    // This allows users to only learn KQL, without any need to learn Observer/Observable composition of pipelines
    // such as Subscribe, IDisposable, and the order of constructing event pipelines
    public class KqlNodeHub
    {
        public KqlNode _node = new KqlNode();
        public IDisposable _outputSubscription;
        public Dictionary<string, IDisposable> _inputSubscriptions = new Dictionary<string, IDisposable>();

        /// <summary>
        /// Create a KqlNodeHub from existing *.csl files. Csl files contain one of more KQL queries as strings.
        /// The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence, observableInput.
        /// </summary>
        /// <param name="observableInput">IObservable<IDictionary<string, object>> - the data that the KQL queries will run against.</param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">string - the name of the subscription.</param>
        /// <param name="fileList">string[] - one or more paths to the *.csl files.</param>
        /// <returns>KqlNodeHub - a KqlNodeHub instance.</returns>
        public static KqlNodeHub FromFiles(
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

            return kqlNodeHub;
        }

        /// <summary>
        /// Create a KqlNodeHub from an existing KQL query as a string. The string must contain only one KQL query.
        /// The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence, observableInput.
        /// </summary>
        /// <param name="observableInput">IObservable<IDictionary<string, object>> - the data that the KQL queries will run against.</param>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        /// <param name="observableName">string - the KQL query as a string.</param>
        /// <param name="fileList">string[] - one or more paths to the *.csl files.</param>
        /// <returns>KqlNodeHub - a KqlNodeHub instance.</returns>
        public static KqlNodeHub FromKqlQuery(
            IObservable<IDictionary<string, object>> observableInput,
            Action<KqlOutput> delegateOutput,
            string observableName,
            string kqlQuery)
        {
            var kqlNodeHub = new KqlNodeHub(delegateOutput);

            kqlNodeHub._node.AddKqlQuery(new CslParagraph
            {
                Query = kqlQuery,
                Description = "Description",
                Comment = "Query"
            });

            kqlNodeHub.AddInput(observableName, observableInput);

            return kqlNodeHub;
        }

        /// <summary>
        /// Create a KqlNodeHub from an existing KQL query as a string. The string must contain only one KQL query.
        /// The KqlNodeHub automatically creates a KqlNode and subscribes the KqlNode to the observable sequence, observableInput.
        /// You can also use the static methods FromFiles() or FromKqlQuery() to instantiate KqlNodeHub.
        /// </summary>
        /// <param name="delegateOutput">Action&lt;KqlOutput&gt; - the structure of the query output as a KqlOutput instance.</param>
        public KqlNodeHub(Action<KqlOutput> delegateOutput)
        {
            _outputSubscription = _node.Subscribe(delegateOutput);
        }

        private void AddInput(string name, IObservable<IDictionary<string, object>> observableInput)
        {
            var subscription = observableInput.Subscribe(_node);
            _inputSubscriptions.Add(name, subscription);
        }
    }
}
