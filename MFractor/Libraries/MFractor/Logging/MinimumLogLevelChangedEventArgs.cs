using System;

namespace MFractor.Logging
{
    public class MinimumLogLevelChangedEventArgs : EventArgs
    {        public MinimumLogLevelChangedEventArgs(LogLevel previous, LogLevel current)
        {
            Previous = previous;
            Current = current;
        }

        public LogLevel Previous { get; }

        public LogLevel Current { get; }
    }
}
