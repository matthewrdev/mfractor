using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Workspace.Data.Synchronisation
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> that provides the <see cref="ITextResourceSynchroniser"/>'s within the app domain.
    /// </summary>
    public interface ITextResourceSynchroniserRepository : IPartRepository<ITextResourceSynchroniser>
    {
        /// <summary>
        /// The <see cref="ITextResourceSynchroniser"/>'s available to the app domain.
        /// </summary>
        IReadOnlyList<ITextResourceSynchroniser>  Synchronisers { get; }
    }
}
