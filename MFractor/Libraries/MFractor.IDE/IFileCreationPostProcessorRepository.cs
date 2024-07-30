using System;
using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Ide
{
    public interface IFileCreationPostProcessorRepository : IPartRepository<IFileCreationPostProcessor>
    {
        IReadOnlyList<IFileCreationPostProcessor> PostProcessors { get; }
    }
}
