using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Images.ImageManager
{
    /// <summary>
    /// A <see cref="IPartRepository{T}"/> for providing the <see cref="IImageManagerCommand"/>'s available in the app domain.
    /// </summary>
    public interface IImageManagerCommandRepository : IPartRepository<IImageManagerCommand>
    {
        /// <summary>
        /// The available <see cref="IImageManagerCommand"/>'s.
        /// </summary>
        IReadOnlyList<IImageManagerCommand> Commands { get; }

        /// <summary>
        /// Get's the <see cref="IImageManagerCommand"/>'s that can execute against the given <paramref name="commandContext"/>.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        IEnumerable<IImageManagerCommand> GetAvailableCommands(IImageManagerCommandContext commandContext);
    }
}
