using System;
using System.Diagnostics;
using System.IO;
using MFractor.Configuration;
using MFractor.Utilities;

namespace MFractor.Logging
{
    public class ApplicationLoggerFactory : ILoggerFactory
    {
        private string logFilePath;
        private LogFileWriter logFileWriter;

        private string metricsFilePath;
        private LogFileWriter metricsFileWriter;
        private readonly int processId;

        private readonly IApplicationPaths applicationPaths;
        private readonly LogLevel minimumLogLevel;
        private readonly IPlatformService platformService;

        public ApplicationLoggerFactory(IProductInformation productInformation, IPlatformService platformService)
        {
            if (productInformation is null)
            {
                throw new ArgumentNullException(nameof(productInformation));
            }

            processId = Process.GetCurrentProcess().Id;

            this.applicationPaths = new ApplicationPaths(new PlatformService());
            this.platformService = platformService;

            var logsFolderPath = Path.Combine(applicationPaths.ApplicationDataPath, LoggingConstants.LogsFolderName);
            if (!Directory.Exists(logsFolderPath))
            {
                Directory.CreateDirectory(logsFolderPath);
            }

            minimumLogLevel = JsonUserOptionsHelper.ReadRawValue(applicationPaths, MinimumLogLevelPreferences.PreferencesKey, LogLevel.Debug.ToString()).ToEnum<LogLevel>();

            var logFileId = DateTime.UtcNow.ToString(LoggingConstants.FileNameDateFormat);

            var logFileName = logFileId + LoggingConstants.LogFileExtension;
            logFilePath = Path.Combine(logsFolderPath, logFileName);

            logFileWriter = new LogFileWriter(logFilePath);
        }

        public ILogger Create(string tag)
        {
            return new ApplicationLogger(tag, logFileWriter, minimumLogLevel, processId);
        }

        public void Dispose()
        {
            if (logFileWriter != null)
            {
                logFileWriter.Dispose();
                logFileWriter = null;
            }

            if (metricsFileWriter != null)
            {
                metricsFileWriter.Dispose();
                metricsFileWriter = null;
            }
        }
    }
}
