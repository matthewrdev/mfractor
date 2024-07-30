using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> implementation that provides the available <see cref="IApplicationLifecycleHandler"/>'s in the app domain.
    /// </summary>
    public interface IApplicationLifecycleHandlerRepository : IPartRepository<IApplicationLifecycleHandler>
    {
        IReadOnlyList<IApplicationLifecycleHandler> ApplicationLifecycleHandlers { get; }
    }
}
