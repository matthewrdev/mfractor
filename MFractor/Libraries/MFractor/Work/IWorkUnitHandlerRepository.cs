using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Work
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> implementation that provides the available <see cref="IWorkUnitHandler"/>'s in the app domain.
    /// </summary>
    public interface IWorkUnitHandlerRepository : IPartRepository<IWorkUnitHandler>
    {
        /// <summary>
        /// The <see cref="IWorkUnitHandler"/>'s that are available to the current program, indexed by their <see cref="Type"/>.
        /// </summary>
        IReadOnlyDictionary<Type, IWorkUnitHandler> WorkUnitHandlers { get; }

        /// <summary>
        /// Is a <see cref="IWorkUnitHandler"/> available for an <see cref="IWorkUnit"/> of <paramref name="type"/>?
        /// </summary>
        bool IsSupportedWorkUnit<TWorkUnit>() where TWorkUnit : IWorkUnit;

        /// <summary>
        /// Is the given <paramref name="workUnit"/> supported by the work engine?
        /// </summary>
        bool IsSupportedWorkUnit(IWorkUnit workUnit);

        /// <summary>
        /// Is a <see cref="IWorkUnitHandler"/> available for an <see cref="IWorkUnit"/> of <paramref name="type"/>?
        /// </summary>
        bool IsSupportedWorkUnit(Type type);

        /// <summary>
        /// Gets the <see cref="IWorkUnitHandler"/> that can handle the provided <paramref name="type"/>.
        /// </summary>
        IWorkUnitHandler GetWorkUnitHandler(Type type);

        /// <summary>
        /// Gets the <see cref="IWorkUnitHandler"/>, cast as <typeparamref name="TWorkUnitHandler"/>, that can handle the provided <paramref name="type"/>.
        /// </summary>
        TWorkUnitHandler GetWorkUnitHandler<TWorkUnitHandler>(Type type) where TWorkUnitHandler : class, IWorkUnitHandler;
    }
}
