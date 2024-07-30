using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Commands
{
    /// <summary>
    /// An <see cref="IPartRepository{T}"/> implementation that provides the available <see cref="ICommand"/>'s in the app domain.
    /// </summary>
    public interface ICommandRepository : IPartRepository<ICommand>
    {
        /// <summary>
        /// The commands.
        /// </summary>
        public IReadOnlyList<ICommand> Commands { get; }
    }
}
