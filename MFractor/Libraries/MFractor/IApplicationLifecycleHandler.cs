using System;
using System.ComponentModel.Composition;

namespace MFractor
{
    /// <summary>
    /// The <see cref="IApplicationLifecycleHandler"/> can be implemented to receive startup and shutdown notifications from MFractor.
    /// <para/>
    /// If a service or sub-system needs to perform some kind of startup, implement this interface and put your code in the <see cref="Startup"/> implementation.
    /// <para/>
    /// If the <see cref="IApplicationLifecycleHandler"/> implemenation is exported multiple times, be sure to use the <see cref="CreationPolicy.Shared"/> settings to ensure a single instance.
    /// <para/>
    /// Implementations of <see cref="IApplicationLifecycleHandler"/> are automatically detected and added into the <see cref="IApplicationLifecycleHandlerRepository"/>.
    /// <para/>
    /// The <see cref="Startup"/> method runs in a background thread. Any UI work should be invoked onto the main thread.
    /// <para/>
    /// You may change the startup order of a <see cref="IApplicationLifecycleHandler"/> by applying the <see cref="Attributes.ApplicationLifecyclePriorityAttribute"/>.
    /// </summary>
    [InheritedExport]
    public interface IApplicationLifecycleHandler
    {
        /// <summary>
        /// Startup this <see cref="IApplicationLifecycleHandler"/>.
        /// </summary>
        void Startup();

        /// <summary>
        /// Shutdown this <see cref="IApplicationLifecycleHandler"/>.
        /// </summary>
        void Shutdown();
    }
}
