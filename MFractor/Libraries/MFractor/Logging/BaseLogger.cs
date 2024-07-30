using System;

namespace MFractor.Logging
{
    public abstract class BaseLogger : ILogger
    {
        public BaseLogger(string tag)
        {
            Tag = tag;
        }

        public void Error(string message)
        {
            Log(Tag, message, LogLevel.Error);
        }

        public void Warning(string message)
        {
            Log(Tag, message, LogLevel.Warning);
        }

        public void Info(string message)
        {
            Log(Tag, message, LogLevel.Information);
        }

        public void Debug(string message)
        {
            Log(Tag, message, LogLevel.Debug);
        }

        public void Verbose(string message)
        {
            Log(Tag, message, LogLevel.Verbose);
        }

        public string Tag { get; }

        public abstract void Log(string tag, string message, LogLevel logLevel = LogLevel.Information);

        public abstract void Exception(Exception ex);
    }
}

