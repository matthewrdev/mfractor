using System;

namespace MFractor.Licensing
{
    class PaidFeatureUsageEventArgs : EventArgs
    {
        public int RemainingCount { get; }

        public int LastRemainingCount { get; }

        public string Message { get; }

        public PaidFeatureUsageEventArgs(int remaining,
                                         int lastRemainingCount,
                                         string message = "")
        {
            RemainingCount = remaining;
            LastRemainingCount = lastRemainingCount;
            Message = message;
        }
    }
}