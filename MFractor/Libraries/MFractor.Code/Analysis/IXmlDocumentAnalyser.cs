using System.Collections.Generic;
using System.Threading;
using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    public interface IXmlDocumentAnalyser
	{
        IReadOnlyList<ICodeIssue> Analyse (IParsedXmlDocument document, 
                                         IFeatureContext context, 
                                         CancellationToken cancellation);

        IReadOnlyList<ICodeIssue> Analyse(IParsedXmlDocument document,
                                        IFeatureContext context,
                                        IReadOnlyList<IXmlSyntaxCodeAnalyser> analysers,
                                        CancellationToken cancellation);

        IReadOnlyList<ICodeIssue> Analyse(IParsedXmlDocument document,
                                        IFeatureContext context,
                                        XmlNode nodeSyntax,
                                        IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> nodeAnalysers,
                                        IReadOnlyDictionary<CodeAnalyserExecutionFilter, IReadOnlyList<IXmlSyntaxCodeAnalyser>> attributeAnalysers,
                                        CancellationToken cancellation);
    }
}

