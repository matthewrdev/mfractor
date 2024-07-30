using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Documents;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// Provides <see cref="ICodeAnalysisPreprocessor"/>'s that are available to the given document.
    /// <para/>
    /// Implementations of <see cref="ICodeAnalysisPreprocessorProvider"/> are automatically registered via MEF.
    /// </summary>
    [InheritedExport]
    public interface ICodeAnalysisPreprocessorProvider
    {
        /// <summary>
        /// Provide the <see cref="ICodeAnalysisPreprocessor"/>'s that are availabe in the given <paramref name="document"/> and <paramref name="context"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        IEnumerable<ICodeAnalysisPreprocessor> ProvidePreprocessors(IParsedXmlDocument document, IFeatureContext context);
    }
}
