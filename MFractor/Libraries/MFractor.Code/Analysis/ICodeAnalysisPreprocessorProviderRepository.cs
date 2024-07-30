using System.Collections.Generic;
using MFractor.IOC;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// A 
    /// </summary>
    public interface ICodeAnalysisPreprocessorProviderRepository : IPartRepository<ICodeAnalysisPreprocessorProvider>
    {
        IReadOnlyList<ICodeAnalysisPreprocessorProvider> CodeAnalysisPreprocessorProviders { get; }
    }
}
