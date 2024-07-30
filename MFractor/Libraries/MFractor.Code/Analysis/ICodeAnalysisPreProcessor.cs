using System;
using MFractor.Code.Documents;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// A <see cref="ICodeAnalysisPreprocessor"/> runs before code analysis to preprocess the document and prepare expensive resolution operations and cache them in memory once.
    /// <para/>
    /// For example, analysis routines that inspect XAML static resources can register a pre-processor to resolve all static resources in a document. This ensures each individual analyser does not need perform the lookup on each analysis iteration.
    /// <para/>
    /// Code analysis preprocessors are manually created and returned via a <see cref="ICodeAnalysisPreprocessorProvider"/>.
    /// <para/>
    /// Preprocessers are attached to the <see cref="FeatureContext"/> as meta-data using their full type name as a key. Use <see cref="CodeAnalyser.TryGetPreprocessor{TPreprocessor}(FeatureContext, out TPreprocessor)"/> to retrieve a preprocessor.
    /// </summary>
    public interface ICodeAnalysisPreprocessor
    {
        /// <summary>
        /// Is this preprocessor in a valid state and should it be used?
        /// <para/>
        /// For example, if this preprocessor depends on the <see cref="MFractor.Data.IProjectResourcesDatabase"/> and the database was not yet synced, the data from the preprocessor should not be trusted for analysis.
        /// <para/>
        /// Using preprocessors when they are invalid can result in incorrect analysis warnings.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Use the given <paramref name="document"/> and <paramref name="context"/> for preprocessing.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        bool Preprocess(IParsedXmlDocument document, IFeatureContext context);
    }
}
