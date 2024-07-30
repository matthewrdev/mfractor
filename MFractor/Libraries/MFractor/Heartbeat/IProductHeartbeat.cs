using System;

namespace MFractor.Heartbeat
{
    /// <summary>
    /// MFractors product heartbeat.
    /// </summary>
    public interface IProductHeartbeat
    {
        /// <summary>
        /// Invoked when MFractors heartbeat is triggered.
        /// <para/>
        /// To see the time elapsed between heartbeats, use <see cref="ProductHeartbeatEventArgs.Elapsed"/>.
        /// <para/>
        /// If your heartbeat delegate throws any kind of exception that is caught by the heartbeat engine, your delegate will be immediately removed. MFractor has zero tolerance on exceptions for heartbeat subscribers. Please see review the output logs for warnings to detect this.
        /// <para/>
        /// Lastly, the thread safety of the heartbeat event is not guaranteed. Please ensure you transition to the main thread using the <see cref="IDispatcher"/> if you need to display UIs.
        /// </summary>
        event EventHandler<ProductHeartbeatEventArgs> Heartbeat;
    }
}
