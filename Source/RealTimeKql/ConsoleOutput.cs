using System;
using System.Collections.Generic;

namespace RealTimeKql
{
    class ConsoleOutput: IObserver<IDictionary<string, object>>
    {
        private bool running = false;
        private bool error = false;
        private bool firstEntry = true;

        public ConsoleOutput()
        {
            running = true;
        }

        public void OnNext(IDictionary<string, object> value)
        {
            if(running)
            {
                if(firstEntry)
                {
                    firstEntry = false;
                    Console.WriteLine(string.Join("\t", value.Keys));
                }

                // printing value to console
                Console.WriteLine(string.Join("\t", value.Values));
            }
        }

        public void OnError(Exception error)
        {
            this.error = true;
        }

        public void OnCompleted()
        {
            running = false;
            if (error != true)
            {
                Console.WriteLine("Completed!");
            }
        }
    }
}
