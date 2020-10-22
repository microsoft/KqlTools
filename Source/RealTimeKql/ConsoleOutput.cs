using System;
using System.Collections.Generic;

namespace RealTimeKql
{
    class ConsoleOutput: IObserver<IDictionary<string, object>>
    {
        private bool error = false;

        public void OnNext(IDictionary<string, object> value)
        {
            // printing value to console
            Console.WriteLine(string.Join("\t", value.Values));
        }

        public void OnError(Exception error)
        {
            this.error = true;
        }

        public void OnCompleted()
        {
            if (error != true)
            {
                Console.WriteLine("Completed!");
            }
        }
    }
}
