using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public class ConsoleJsonOutput : IOutput
    {
        private bool _running;
        public ConsoleJsonOutput()
        {
            _running = true;
        }

        public void Stop()
        {
            _running = false;
            Console.WriteLine("\nCompleted!");
            Console.WriteLine("Thank you for using RealTimeKql!");
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_running)
            {
                Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }
        }

        public void OutputError(Exception ex)
        {
            _running = false;
            Console.WriteLine(ex);
        }
    }
}
