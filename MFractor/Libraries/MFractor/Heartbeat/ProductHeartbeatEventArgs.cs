using System;

namespace MFractor.Heartbeat
{
    /// <summary>
    /// The event arguments for when a <see cref="IProductHeartbeat.Heartbeat"/> occurs.
    /// <para/>
    /// Includes the <see cref="Elapsed"/> time between this heartbeat event and the previous heart beat event.
    /// </summary>
    public class ProductHeartbeatEventArgs : EventArgs
    {
        public ProductHeartbeatEventArgs(TimeSpan elapsed)
        {
            Elapsed = elapsed;
        }

        /// <summary>
        /// The amout of time elapsed between this heartbeat event and the previous heart beat event.
        /// </summary>
        public TimeSpan Elapsed { get; }
    }
}
