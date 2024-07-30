using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Data.GarbageCollection
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="IRepositoryCollection"/>'s within the app domain.
    /// </summary>
    public interface IGarbageCollectionEventsRepository : IPartRepository<IGarbageCollectionEvents>
    {
        /// <summary>
        /// The available <see cref="IGarbageCollectionEvents"/> registrations within the app domain.
        /// </summary>
        IReadOnlyList<IGarbageCollectionEvents> GarbageCollectionEvents { get; }
    }
}
