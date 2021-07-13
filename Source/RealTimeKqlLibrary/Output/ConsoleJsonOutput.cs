using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reactive.Kql.CustomTypes;

namespace RealTimeKqlLibrary
{
    public class ConsoleJsonOutput : IOutput
    {
        private bool _running;
        private readonly BaseLogger _logger;
        
        public ConsoleJsonOutput(BaseLogger logger)
        {
            _running = true;
            _logger = logger;
        }

        public void KqlOutputAction(KqlOutput obj)
        {
            OutputAction(obj.Output);
        }

        public void OutputAction(IDictionary<string, object> obj)
        {
            if (_running)
            {
                try
                {
                    Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }));
                }
                catch(Exception ex)
                {
                    OutputError(ex);
                }
            }
        }

        public void OutputError(Exception ex)
        {
            _running = false;
            _logger.Log(LogLevel.ERROR, ex);
        }

        public void OutputCompleted()
        {
            _running = false;
            _logger.Log(LogLevel.INFORMATION, "Stopping RealTimeKql...");
        }

        public void Stop()
        {
            _logger.Log(LogLevel.INFORMATION, $"\nCompleted!\nThank you for using RealTimeKql!");
        }
    }
}
