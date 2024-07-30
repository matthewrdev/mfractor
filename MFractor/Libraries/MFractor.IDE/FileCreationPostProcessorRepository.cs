using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Ide;
using MFractor.IOC;

namespace MFractor.Ide
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFileCreationPostProcessorRepository))]
    public class FileCreationPostProcessorRepository : PartRepository<IFileCreationPostProcessor>, IFileCreationPostProcessorRepository
    {
        [ImportingConstructor]
        public FileCreationPostProcessorRepository(Lazy<IPartResolver> partResolver) : base(partResolver)
        {
        }

        public IReadOnlyList<IFileCreationPostProcessor> PostProcessors => Parts;
    }
}