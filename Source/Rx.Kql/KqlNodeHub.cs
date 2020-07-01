using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;
using System.Text;

namespace System.Reactive.Kql
{
    // Class which completely hides the usage of Reactive Extensions
    // This allows users to only learn KQL, withou any need to learn Observer/Observable composition of pipelines
    // such as Subscribe, IDisposable, and the order of constructing event pipelines
    public class KqlNodeHub
    {
        public KqlNode _node = new KqlNode();
        public IDisposable _outputSubscription;
        public Dictionary<string, IDisposable> _inputSubscriptions = new Dictionary<string, IDisposable>();

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
