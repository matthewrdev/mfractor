using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Workspace.Data.Synchronisation
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="IAssetResourceSynchroniser"/>'s within the app domain.
    /// </summary>
    public interface IAssetResourceSynchroniserRepository : IPartRepository<IAssetResourceSynchroniser>
    {
        /// <summary>
        /// The <see cref="IAssetResourceSynchroniser"/>'s available to the app domain.
        /// </summary>
        IReadOnlyList<IAssetResourceSynchroniser> Synchronisers { get; }
    }
}
