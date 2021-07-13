using System;
using System.Collections.Generic;

namespace RealTimeKqlLibrary
{
    public abstract class BaseLogger
    {
        protected Dictionary<int, bool> _logLevels;
        
        public BaseLogger()
        {
            _logLevels = new Dictionary<int, bool>();
            foreach(int level in Enum.GetValues(typeof(LogLevel)))
            {
                _logLevels.Add(level, true);
            }
        }

        public abstract void Log(LogLevel logLevel, object payload);

        public void Disable(LogLevel logLevel)
        {
            if (!_logLevels.ContainsKey((int)logLevel)) return;
            _logLevels[(int)logLevel] = false;
        }

        public void Enable(LogLevel logLevel)
        {
            if (!_logLevels.ContainsKey((int)logLevel)) return;
            _logLevels[(int)logLevel] = true;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            var success = _logLevels.TryGetValue((int)logLevel, out bool value);
            if (!success) return false;
            return value;
        }
    }

    public enum LogLevel
    {
        INFORMATION,
        ERROR,
        DEBUG
    };
}
