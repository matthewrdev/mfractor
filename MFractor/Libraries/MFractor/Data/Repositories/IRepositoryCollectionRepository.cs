using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Data.Repositories
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="IRepositoryCollection"/>'s within the app domain.
    /// </summary>
    public interface IRepositoryCollectionRepository : IPartRepository<IRepositoryCollection>
    {
        /// <summary>
        /// The available <see cref="IRepositoryCollection"/> registrations within the app domain.
        /// </summary>
        IReadOnlyList<IRepositoryCollection> RepositoryCollections { get; }
    }
}
