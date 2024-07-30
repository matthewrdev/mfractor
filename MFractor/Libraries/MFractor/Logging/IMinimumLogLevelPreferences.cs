using System;

namespace MFractor.Logging
{
    public interface IMinimumLogLevelPreferences
    {
        LogLevel MinimumLogLevel { get; }

        void ChangeMinimumLogLevel(LogLevel logLevel);

        event EventHandler<MinimumLogLevelChangedEventArgs> OnMinimumLogLevelChanged;
    }
}
