using System;
using System.Collections.Generic;
using System.Text;

namespace RealTimeKqlLibrary
{
    public class ConsoleLogger : BaseLogger
    {
        public ConsoleLogger() : base() { }

        public override void Log(LogLevel logLevel, object payload)
        {
            if (!IsEnabled(logLevel)) return;
            Console.WriteLine(payload);
        }
    }
}
