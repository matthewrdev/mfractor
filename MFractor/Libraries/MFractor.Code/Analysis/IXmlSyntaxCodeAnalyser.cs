using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// An MFractor code analyser.
    /// </summary>
    [InheritedExport]
    public interface IXmlSyntaxCodeAnalyser : IConfigurable
    {
        /// <summary>
        /// The ID that maps to <see cref="Microsoft.CodeAnalysis.DiagnosticDescriptor.Id"/>.
        /// <para/>
        /// Must be formatted as MF####.
        /// <para/>
        /// MFractors diagnostic analysers are mapped to the following ranges:
        /// <para>MF1000-1999: XAML</para>
        /// <para>MF2000-2999: C#</para>
        /// <para>MF3000-3999: Android Xml</para>
        /// </summary>
        string DiagnosticId { get; }

        /// <summary>
        /// Gets the type of the code issue.
        /// </summary>
		IssueClassification Classification { get; }

        /// <summary>
        /// A filter to quickly include or exclude this analyser analysing particular file types.
        /// </summary>
        CodeAnalyserExecutionFilter Filter { get; }

        /// <summary>
        /// The xml element that this analyser should inspect.
        /// </summary>
        XmlSyntaxKind TargetSyntax { get; }

        /// <summary>
        /// If the analysis routine is interested in analysing a specific document.
        /// <para/>
        /// Use this to reject an analyser based on a cheap evaulation condition.
        /// <para/>
        /// For instance, if a certain namespace is expected to be present, returning false can skip unnecessary validation that an analyser
        /// would either have to do in an Analyse method.
        /// </summary>
        /// <returns>True if the analyser would like to analyse the document, false if this analyser can be skipped.</returns>
        /// <param name="document">The document being analysed.</param>
        /// <param name="context">The abstract bundle that contains third-party data/objects/services for the analyser.</param>
        bool IsInterestedInDocument(IParsedXmlDocument document, IFeatureContext context);

        /// <summary>
        /// Analyse the specific xml node using the document, bundle and syntax.
        /// </summary>
        /// <param name="syntax">The node to analyse.</param>
        /// <param name="document">The document being analysed.</param>
        /// <param name="context">The abstract bundle that contains third-party data/objects/services for the analyser.</param>
        IReadOnlyList<ICodeIssue> AnalyseSyntax(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context);

        /// <summary>
        /// Analyse the specific xml attribute using the document, bundle and selectorPath.
        /// </summary>
        /// <param name="syntax">The attribute to analyse.</param>
        /// <param name="document">The document being analysed.</param>
        /// <param name="context">The abstract bundle that contains third-party data/objects/services for the analyser.</param>
        IReadOnlyList<ICodeIssue> AnalyseSyntax(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context);
    }
}
