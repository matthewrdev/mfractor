using System;
namespace MFractor.Logging
{
    public static class LoggingConstants
    {
        public const string FileNameDateFormat = "yyyy-MM-ddTHH-mm-ss";

        public const string LogFileExtension = ".log";

        public const string MetricsFileExtension = ".metrics.log";

        public static readonly TimeSpan RetainedLogsMaximumTime = TimeSpan.FromDays(14);

        public const string LogsFolderName = "logs";
    }
}
