using System;
using System.Collections.Generic;

namespace MFractor.Logging
{
    public static class LogFormatter
    {
        private const string dateTimeFormat = "MM-dd HH:mm:ss.fff";

        private static readonly IReadOnlyDictionary<LogLevel, string> Levels = new Dictionary<LogLevel, string>()
        {
            { LogLevel.Verbose    , "V" },
            { LogLevel.Debug      , "D" },
            { LogLevel.Information, "I" },
            { LogLevel.Warning    , "W" },
            { LogLevel.Error      , "E" },
            { LogLevel.Fatal      , "F" },
        };

        /// <summary>
        /// Renders the given parameters into a threadtime formatted log entry.
        /// <para/>
        /// <see cref="https://developer.android.com/studio/command-line/logcat#outputFormat"/>
        /// </summary>
        public static string Render(DateTime dateTime, string tag, string message, int processId, int threadId, LogLevel logLevel)
        {
            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException($"'{nameof(tag)}' cannot be null or empty.", nameof(tag));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException($"'{nameof(message)}' cannot be null or empty.", nameof(message));
            }

            var level = Levels[logLevel];
            return $"{dateTime.ToString(dateTimeFormat)} {processId} {threadId} {level} {tag}: {message}";
        }
    }
}
