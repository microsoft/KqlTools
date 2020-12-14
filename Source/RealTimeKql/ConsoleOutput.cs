using System;
using System.Collections.Generic;

namespace RealTimeKql
{
    class ConsoleOutput: IObserver<IDictionary<string, object>>
    {
        private bool _running = false;
        private bool _error = false;
        private bool _firstEntry = true;
        private bool _tableFormat = false;

        public ConsoleOutput(bool tableFormat)
        {
            _running = true;
            _tableFormat = tableFormat;

            // DEBUG
            Console.WriteLine("tableFormat: {0}", tableFormat);
        }

        public void OnNext(IDictionary<string, object> value)
        {
            if(_running)
            {
                if(_firstEntry)
                {
                    _firstEntry = false;
                    Console.WriteLine(string.Join("\t", value.Keys));
                }

                // printing value to console
                Console.WriteLine(string.Join("\t", value.Values));
            }
        }

        public void OnError(Exception error)
        {
            this._error = true;
        }

        public void OnCompleted()
        {
            _running = false;
            if (_error != true)
            {
                Console.WriteLine("Completed!");
            }
        }
    }
}
