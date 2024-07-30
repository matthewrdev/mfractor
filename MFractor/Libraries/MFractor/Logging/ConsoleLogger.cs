using System;
using System.Diagnostics;
using System.Threading;

namespace MFractor.Logging
{
    public class ConsoleLogger : BaseLogger
    {
        public int ProcessId { get; }

        public ConsoleLogger(string context)
            : base(context)
        {
            ProcessId = Process.GetCurrentProcess().Id;
        }

        public override void Log(string tag, string message, LogLevel logLevel)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var now = DateTime.Now;
            var lines = message.Split('\n', '\r');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var output = LogFormatter.Render(now, tag, line, ProcessId, Thread.CurrentThread.ManagedThreadId, logLevel);
                Console.Out.WriteLine(output);
            }
        }

        public override void Exception(Exception ex)
        {
            Log(Tag, ex.ToString(), LogLevel.Error);
        }
    }
}

