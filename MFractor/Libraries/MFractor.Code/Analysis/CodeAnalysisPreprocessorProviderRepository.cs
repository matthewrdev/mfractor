using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.IOC;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeAnalysisPreprocessorProviderRepository))]
    class CodeAnalysisPreprocessorProviderRepository : PartRepository<ICodeAnalysisPreprocessorProvider>, ICodeAnalysisPreprocessorProviderRepository
    {
        [ImportingConstructor]
        public CodeAnalysisPreprocessorProviderRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        public IReadOnlyList<ICodeAnalysisPreprocessorProvider> CodeAnalysisPreprocessorProviders => Parts;
    }
}