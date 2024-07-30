using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Workspace.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITextResourceSynchroniserRepository))]
    class TextResourceSynchroniserRepository : PartRepository<ITextResourceSynchroniser>, ITextResourceSynchroniserRepository
    {
        [ImportingConstructor]
        public TextResourceSynchroniserRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ITextResourceSynchroniser> Synchronisers => Parts;
    }
}