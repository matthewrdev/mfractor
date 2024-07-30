using System.Collections.Generic;
using MFractor.Code.Documents;
using MFractor.IOC;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// Code analyser repository.
    /// </summary>
    public interface ICodeAnalyserRepository : IPartRepository<IXmlSyntaxCodeAnalyser>
	{
        /// <summary>
        /// Gets the code analysers.
        /// </summary>
        /// <value>The code analysers.</value>
        IReadOnlyList<IXmlSyntaxCodeAnalyser> Analysers { get; }

        /// <summary>
        /// Gets the code analylsers that support the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IEnumerable<IXmlSyntaxCodeAnalyser> GetCodeAnalysersForSyntaxKind (XmlSyntaxKind type);

        IEnumerable<IXmlSyntaxCodeAnalyser> GetCodeAnalysersForSyntaxKindAndScope(XmlSyntaxKind type, CodeAnalyserExecutionFilter scope);

        IXmlSyntaxCodeAnalyser GetByIdentifier(string identifier);

        IXmlSyntaxCodeAnalyser GetByDiagnosticId(string diagnosticId);
    }
}

