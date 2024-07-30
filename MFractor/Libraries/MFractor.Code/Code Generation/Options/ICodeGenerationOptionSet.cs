using System;
using System.Collections.Generic;

namespace MFractor.Code.CodeGeneration.Options
{
    public interface ICodeGenerationOptionSet : IEnumerable<ICodeGenerationOption>
    {
        IReadOnlyList<ICodeGenerationOption> Options { get; }
    }
}
