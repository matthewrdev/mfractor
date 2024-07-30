using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading;

namespace MFractor.Heartbeat
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProductHeartbeat))]
    class ProductHeartbeat : IProductHeartbeat, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public event EventHandler<ProductHeartbeatEventArgs> Heartbeat;

        readonly Lazy<EventInfo> heartBeatEventInfo = new Lazy<EventInfo>(() => typeof(ProductHeartbeat).GetEvent(nameof(Heartbeat)));
        EventInfo HeartBeatEventInfo => heartBeatEventInfo.Value;

        public void Shutdown()
        {
            try
            {
                if (source != null)
                {
                    source.Cancel();
                    source = null;
                }

                if (heartbeatThread != null)
                {
                    heartbeatThread.Abort();
                    heartbeatThread = null;
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Startup()
        {
            source = new CancellationTokenSource();
            token = source.Token;
            heartbeatThread = new Thread(RunHeartbeat);
            heartbeatThread.Start();
        }

        CancellationTokenSource source;
        CancellationToken token;
        Thread heartbeatThread;

        void RunHeartbeat(object obj)
        {
            var then = DateTime.UtcNow;
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                var now = DateTime.UtcNow;

                var args = new ProductHeartbeatEventArgs(now - then);
                then = now;

                NotifyAll(args);
            }
        }

        void NotifyAll(ProductHeartbeatEventArgs eventArgs)
        {
            if (eventArgs == null)
            {
                return;
            }

            try
            {
                var invocations = Heartbeat?.GetInvocationList();

                if (invocations == null)
                {
                    return;
                }

                foreach (var handler in invocations)
                {
                    if (handler == null)
                    {
                        continue;
                    }

                    try
                    {
                        handler.DynamicInvoke(this, eventArgs);
                    }
                    catch (Exception ex)
                    {
                        log?.Warning("The heartbeat delegate for " + handler.Method.Name + " from " + handler.Target.ToString() + " threw " + ex);
                        log?.Warning("MFractor has zero tolerance for heartbeat delegates that throw exceptions, please ensure your delegate handles all exceptions. Automatically removing.");
                        RemoveHeartbeatHandler(handler);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void RemoveHeartbeatHandler(Delegate handler)
        {
            try
            {
                HeartBeatEventInfo.RemoveEventHandler(this, handler);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}