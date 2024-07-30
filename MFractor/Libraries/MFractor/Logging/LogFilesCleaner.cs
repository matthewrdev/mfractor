using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MFractor.Logging
{
    /// <summary>
    /// An <see cref="IApplicationLifecycleHandler"/> implementation that checks the applications logs directory and cleans up old log files.
    /// </summary>
    class LogFilesCleaner : IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        private readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        [ImportingConstructor]
        public LogFilesCleaner(Lazy<IApplicationPaths> applicationPaths)
        {
            this.applicationPaths = applicationPaths;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            var logsFolder = Path.Combine(ApplicationPaths.ApplicationDataPath, LoggingConstants.LogsFolderName);
            if (!Directory.Exists(logsFolder))
            {
                return;
            }

            var logs = Directory.GetFiles(logsFolder, "*" + LoggingConstants.LogFileExtension);

            if (logs.Length == 0)
            {
                return;
            }

            foreach (var file in logs)
            {
                var logDateTimeValue = file.Split('.').FirstOrDefault();

                if (string.IsNullOrEmpty(logDateTimeValue))
                {
                    continue;
                }

                if (!DateTime.TryParseExact(logDateTimeValue, LoggingConstants.FileNameDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
                {
                    continue;
                }

                if (time < DateTime.UtcNow - LoggingConstants.RetainedLogsMaximumTime)
                {
                    log?.Debug($"Deleting expired log file: '{file}'");
                    File.Delete(file);
                }
            }
        }
    }
}
