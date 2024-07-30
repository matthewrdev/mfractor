using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;

namespace MFractor.Images.ImageManager
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageManagerCommandRepository))]
    class ImageManagerCommandRepository : PartRepository<IImageManagerCommand>, IImageManagerCommandRepository
    {
        [ImportingConstructor]
        public ImageManagerCommandRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<IImageManagerCommand> Commands => Parts;

        public IEnumerable<IImageManagerCommand> GetAvailableCommands(IImageManagerCommandContext commandContext)
        {
            return Commands.Where(c =>
            {
                var state = c.GetExecutionState(commandContext);

                return state != null && state.CanExecute;
            });
        }
    }
}