using System;
using System.ComponentModel.Composition;
using MFractor.Concurrency;
using MFractor.Configuration;

namespace MFractor.Logging
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMinimumLogLevelPreferences))]
    class MinimumLogLevelPreferences : IMinimumLogLevelPreferences, IApplicationLifecycleHandler
    {
        readonly ILogger log = Logging.Logger.Create();

        public const string PreferencesKey = "com.mfractor.preferences.minimum_log_level";

        private readonly ConcurrentValue<LogLevel> minimumLogLevel = new ConcurrentValue<LogLevel>(LogLevel.Debug);
        public LogLevel MinimumLogLevel => minimumLogLevel.Get();

        private readonly Lazy<IUserOptions> userOptions;
        public IUserOptions UserOptions => userOptions.Value;

        public event EventHandler<MinimumLogLevelChangedEventArgs> OnMinimumLogLevelChanged;

        [ImportingConstructor]
        public MinimumLogLevelPreferences(Lazy<IUserOptions> userOptions)
        {
            this.userOptions = userOptions;
        }

        public void Shutdown()
        {
        }

        public void Startup()
        {
            if (!UserOptions.HasKey(PreferencesKey))
            {
                log?.Info($"No minimum log level is defined in the users preferences. Defaulting to {LogLevel.Debug}");
                UserOptions.Set(PreferencesKey, LogLevel.Debug);
            }

            minimumLogLevel.Set(UserOptions.Get(PreferencesKey, LogLevel.Debug));
            log?.Info($"The minimum log level is: {minimumLogLevel.Get()}");
        }

        public void ChangeMinimumLogLevel(LogLevel logLevel)
        {
            if (MinimumLogLevel == logLevel)
            {
                return;
            }

            var previous = MinimumLogLevel;
            this.minimumLogLevel.Set(logLevel);

            log?.Info($"Changing minimum log level: {previous} to {logLevel}");
            log?.Info($"For changes to fully take effect, please restart.");
            UserOptions.Set(PreferencesKey, logLevel);

            OnMinimumLogLevelChanged?.Invoke(this, new MinimumLogLevelChangedEventArgs(previous, logLevel));
        }
    }
}