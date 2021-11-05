using System;
using System.Collections.Generic;
using System.Reactive.Kql;
using System.IO;
using System.Reactive.Kql.EventTypes;

namespace RealTimeKqlLibrary
{
    public class EventProcessor
    {
        private readonly IObservable<IDictionary<string, object>> _inputStream;
        private readonly string _inputName;
        private readonly IOutput _output;
        private readonly bool _realTimeMode;
        private readonly string[] _queries;
        private KqlNodeHub _kqlNodeHub;

        public EventProcessor(
            IObservable<IDictionary<string, object>> inputStream,
            string inputName,
            IOutput output,
            bool realTimeMode,
            params string[] queries)
        {
            _inputStream = inputStream;
            _inputName = inputName;
            _output = output;
            _realTimeMode = realTimeMode;
            _queries = queries;
            _kqlNodeHub = null;
        }

        public bool ApplyRxKql()
        {
            List<string> queriesFullPath = new List<string>();
            foreach(var query in _queries)
            {
                if(!string.IsNullOrEmpty(query))
                {
                    var queryFullPath = Path.GetFullPath(query);
                    if (!File.Exists(queryFullPath))
                    {
                        _output.OutputError(new Exception($"ERROR! Query file {queryFullPath} does not seem to exist."));
                        return false;
                    }
                    else
                    {
                        queriesFullPath.Add(queryFullPath);
                    }
                }
            }
            
            // adding custom functions
            if(_realTimeMode)
            {
                ScalarFunctionFactory.AddFunctions(typeof(RealTimeCustomScalarFunctions));
            }
            ScalarFunctionFactory.AddFunctions(typeof(CustomScalarFunctions));

            // instantiating KqlNodeHub with input stream, output action, and query files
            _kqlNodeHub = KqlNodeHub.FromFiles(_inputStream, _output.KqlOutputAction, _inputName, queriesFullPath.ToArray());

            // checking if any queries failed
            foreach(var query in _kqlNodeHub._node.FailedKqlQueryList)
            {
                _output.OutputError(query.FailureReason);
            }

            // return false if any queries failed
            if (_kqlNodeHub._node.FailedKqlQueryList.Count > 0) return false;

            // set up error handling for any runtime query errors
            _kqlNodeHub._node.KqlKqlQueryFailed += KqlQueryFailedEventHandler;
            _kqlNodeHub._node.EnableFailedKqlQueryEvents = true;

            return true;
        }

        public void Stop()
        {
            if(_kqlNodeHub != null && _kqlNodeHub._outputSubscription != null)
            {
                _kqlNodeHub._outputSubscription.Dispose();
            }
        }

        private void KqlQueryFailedEventHandler(object sender, KqlQueryFailedEventArgs e)
        {
            _output.OutputError(e.Exception);
        }
    }
}
