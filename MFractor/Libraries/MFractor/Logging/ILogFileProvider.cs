using System;

namespace MFractor.Logging
{
    public interface ILogFileProvider
    {
        string LogDirectory { get; }

        string CurrentLogFile { get; }
    }
}