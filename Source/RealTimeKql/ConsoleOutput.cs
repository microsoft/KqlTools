using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Kql;
using System.Threading;


namespace RealTimeKql
{
    class ConsoleOutput: IObserver<IDictionary<string, object>>
    {
        public AutoResetEvent Completed { get; private set; }
        private bool error = false;
        private string[] _fields;

        public ConsoleOutput()
        {
            Completed = new AutoResetEvent(false);
        }

        public void OnNext(IDictionary<string, object> value)
        {
            // discover fields from the first event we see
            if(_fields == null)
            {
                _fields = value.Keys.ToArray();
                Console.WriteLine(string.Join("\t", _fields));
            }

            // printing value to console
            Console.WriteLine(string.Join("\t", value.Values));
        }

        public void OnError(Exception error)
        {
            RxKqlEventSource.Log.LogException(error.ToString());
            this.error = true;
        }

        public void OnCompleted()
        {
            if (error != true)
            {
                Console.WriteLine("Completed!");
                Completed.Set();
            }
        }
    }
}
