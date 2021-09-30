using System;

namespace RealTimeKqlLibrary
{
    public class ConsoleLogger : BaseLogger
    {
        public override bool Setup() { return true;  }
        public override void Log(LogLevel logLevel, object payload)
        {
            if (!IsEnabled(logLevel) || logLevel == LogLevel.NONE) return;

            string level = "VERBOSE";
            switch(logLevel)
            {
                case LogLevel.CRITICAL:
                    level = "CRITICAL";
                    break;
                case LogLevel.ERROR:
                    level = "ERROR";
                    break;
                case LogLevel.WARNING:
                    level = "WARNING";
                    break;
                case LogLevel.INFORMATION:
                    level = "INFORMATION";
                    break;
            }

            Console.WriteLine($"{level}:{payload}");
        }
    }
}
