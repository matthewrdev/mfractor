using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using MFractor.Analytics;
using MFractor.IOC;

namespace MFractor.Logging
{
    public class ApplicationLogger : ILogger
    {
        private readonly LogFileWriter logFileWriter;
        private readonly LogLevel minimumLogLevel;

        public ApplicationLogger(string tag, LogFileWriter logFileWriter, LogLevel minimumLogLevel, int processId)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException($"'{nameof(tag)}' cannot be null or whitespace.", nameof(tag));
            }

            Tag = tag;
            this.logFileWriter = logFileWriter ?? throw new ArgumentNullException(nameof(logFileWriter));
            this.minimumLogLevel = minimumLogLevel;
            ProcessId = processId;
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
        public int ProcessId { get; }

        public void Log(string tag, string message, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(message)
                || logLevel < minimumLogLevel)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(tag))
            {
                tag = this.Tag;
            }

            var now = DateTime.Now;
            var lines = message.Split('\n', '\r');

            var logStringBuilder = new StringBuilder();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var output = LogFormatter.Render(now, tag, line, ProcessId, Thread.CurrentThread.ManagedThreadId, logLevel);
                logStringBuilder.AppendLine(output);
            }

            var logContent = logStringBuilder.ToString();

            System.Console.Out.Write(logContent);
            logFileWriter.WriteToFile(logContent);
        }

        public void Exception(Exception ex)
        {
            Log(Tag, ex.ToString(), LogLevel.Error);

            if (Resolver.TryResolve<IAnalyticsService>(out var analytics))
            {
                analytics.Track(ex);
            }
        }
    }
}
