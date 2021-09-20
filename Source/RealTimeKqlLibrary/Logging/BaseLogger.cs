namespace RealTimeKqlLibrary
{
    public abstract class BaseLogger
    {
        private int _maxVerbosityLevel = 5;
        public abstract bool Setup();

        public abstract void Log(LogLevel logLevel, object payload);

        public void SetMaxVerbosity(LogLevel logLevel)
        {
            _maxVerbosityLevel = (int)logLevel;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if ((int)logLevel > _maxVerbosityLevel) return false;
            else return true;
        }
    }

    public enum LogLevel
    {
        NONE = 0,
        CRITICAL = 1,
        ERROR = 2,
        WARNING = 3,
        INFORMATION = 4,
        VERBOSE = 5
    };
}
