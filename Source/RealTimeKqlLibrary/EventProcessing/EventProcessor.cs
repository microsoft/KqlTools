using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;
using System.Reactive.Kql;
using System.IO;

namespace RealTimeKqlLibrary
{
    public class EventProcessor
    {
        private readonly IObservable<IDictionary<string, object>> _inputStream;
        private readonly string _inputName;
        private readonly Action<KqlOutput> _output;
        private readonly bool _realTimeMode;
        private readonly string[] _queries;
        private KqlNodeHub _kqlNodeHub;

        public EventProcessor(
            IObservable<IDictionary<string, object>> inputStream,
            string inputName,
            Action<KqlOutput> output,
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
                        Console.WriteLine($"ERROR! Query file {queryFullPath} does not seem to exist.");
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
            _kqlNodeHub = KqlNodeHub.FromFiles(_inputStream, _output, _inputName, queriesFullPath.ToArray());

            return true;
        }

        public void Stop()
        {
            if(_kqlNodeHub != null)
            {
                _kqlNodeHub._outputSubscription.Dispose();
            }
        }
    }
}
