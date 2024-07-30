using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICommandRepository))]
    class CommandRepository : PartRepository<ICommand>, ICommandRepository
    {
        [ImportingConstructor]
        public CommandRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ICommand> Commands => Parts;
    }
}