﻿using System;

namespace MFractor.Logging
{
    public class MinimumLogLevelChangedEventArgs : EventArgs
    {
        {
            Previous = previous;
            Current = current;
        }

        public LogLevel Previous { get; }

        public LogLevel Current { get; }
    }
}