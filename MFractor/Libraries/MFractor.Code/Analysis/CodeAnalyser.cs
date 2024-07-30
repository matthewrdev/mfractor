using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Code.Documents;
using MFractor.Xml;
using Microsoft.CodeAnalysis.Text;
using System;
using MFractor.Utilities;

namespace MFractor.Code.Analysis
{
    /// <summary>
    /// The base class for an MFractor code analyser.
    /// <para/>
    /// Classes that implement <see cref="CodeAnalyser"/> are single instance and are automatically registered with the <see cref="ICodeAnalyserRepository"/>.
    /// </summary>
    [SuppressConfiguration]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public abstract class CodeAnalyser : Configurable, IXmlSyntaxCodeAnalyser
    {
        protected bool TryGetPreprocessor<TPreprocessor>(IFeatureContext context, out TPreprocessor result) where TPreprocessor : ICodeAnalysisPreprocessor
        {
            result = default;
            var key = typeof(TPreprocessor);

            if (context.MetaData.TryGetValue(key.FullName, out var value)
                && value is TPreprocessor preprocessor)
            {
                result = preprocessor;
                return preprocessor.IsValid;
            }

            return false;
        }

        /// <summary>
        /// If the analysis routine is interested in analysing a specific document.
        /// <para/>
        /// Use this to reject an analyser based on a cheap evaulation condition.
        /// <para/>
        /// For instance, if a certain namespace is expected to be present, returning false can skip unnecessary validation that an analyser would either have to do in an Analyse method.
        /// </summary>
        /// <returns>True if the analyser would like to analyse the document, false if this analyser can be skipped.</returns>
        /// <param name="document">The document being analysed.</param>
        /// <param name="context">The abstract bundle that contains third-party data/objects/services for the analyser.</param>
        public bool IsInterestedInDocument(IParsedXmlDocument document, IFeatureContext context)
        {
            return CanAnalyseDocument(document, context);
        }

        /// <summary>
        /// Can this <paramref name="document"/> and <paramref name="context"/> be analysed?
        /// </summary>
        /// <returns><c>true</c>, if the <paramref name="document"/> can be analysed, <c>false</c> otherwise.</returns>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public virtual bool CanAnalyseDocument(IParsedXmlDocument document,
                                               IFeatureContext context)
        {
            return true;
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public IReadOnlyList<ICodeIssue> AnalyseSyntax(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context)
        {
            return Analyse(syntax, document, context);
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        public IReadOnlyList<ICodeIssue> AnalyseSyntax(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context)
        {
            return Analyse(syntax, document, context);
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        protected virtual IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXmlDocument document, IFeatureContext context)
        {
            return Array.Empty<ICodeIssue>();
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        /// <returns>The analyse.</returns>
        /// <param name="syntax">Syntax.</param>
        /// <param name="document">Document.</param>
        /// <param name="context">Context.</param>
        protected virtual IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context)
        {
            return Array.Empty<ICodeIssue>();
        }

        /// <summary>
        /// Gets the type of the code issue.
        /// </summary>
        /// <value>The type of the code issue.</value>
        public abstract IssueClassification Classification { get; }

        /// <summary>
        /// A filter to quickly include or exclude this analyser analysing particular file types.
        /// </summary>
        /// <value>The scope.</value>
        public abstract CodeAnalyserExecutionFilter Filter { get; }

        /// <summary>
        /// The xml element that this analyser should inspect.
        /// </summary>
        /// <value>The target element type for analysis.</value>
        public abstract XmlSyntaxKind TargetSyntax { get; }

        /// <summary>
        /// The ID that maps to <see cref="Microsoft.CodeAnalysis.DiagnosticDescriptor.Id"/>.
        /// <para/>
        /// Must be formatted as MF####.
        /// <para/>
        /// MFractors diagnostic analysers are mapped to the following ranges:
        /// <para>MF1000-1999: C#</para>
        /// <para>MF2000-2999: XAML</para>
        /// <para>MF3000-3999: Android</para>
        /// <para>MF4000-4999: iOS</para>
        /// </summary>
        public abstract string DiagnosticId { get; }

        /// <summary>
        /// Is this code issue a silent warning, aka, should it trigger the tooltip to display and a code annotation to apply?
        /// <para/>
        /// Silent tooltips detect issues that then activate fixes/refactorings without appearing in the user interface.
        /// </summary>
        protected virtual bool IsSilent => false;

        /// <summary>
        /// A factory method to create a new code issue.
        /// </summary>
        /// <returns>The issue.</returns>
        /// <param name="message">Message.</param>
        /// <param name="node">Node.</param>
        /// <param name="span">Span.</param>
        /// <param name="additionalContent">Additional content.</param>
        protected ICodeIssue CreateIssue(string message, XmlNode node, TextSpan span, object additionalContent = null)
        {
            var issue = new CodeIssue(this.GetType(), this.DiagnosticId, this.Identifier, message, span, this.Classification, IsSilent, node);

            if (additionalContent != null)
            {
                issue.SetAdditionalContent(additionalContent);
            }

            AttachResultsToElement(node, new List<ICodeIssue>() { issue });

            return issue;
        }

        /// <summary>
        /// A factory method to create a new code issue.
        /// </summary>
        /// <returns>The issue.</returns>
        /// <param name="message">Message.</param>
        /// <param name="attribute">Attribute.</param>
        /// <param name="span">Span.</param>
        /// <param name="additionalContent">Additional content.</param>
        protected ICodeIssue CreateIssue(string message, XmlAttribute attribute, TextSpan span, object additionalContent = null)
        {
            var issue = new CodeIssue(this.GetType(), this.DiagnosticId, this.Identifier, message, span, this.Classification, IsSilent, attribute);

            if (additionalContent != null)
            {
                issue.SetAdditionalContent(additionalContent);
            }

            AttachResultsToElement(attribute, new List<ICodeIssue>() { issue });

            return issue;
        }

        void AttachResultsToElement(XmlNode node, List<ICodeIssue> analysisResults)
        {
            if (!node.HasKey(MetaDataKeys.Analysis.Issues))
            {
                node.Add(MetaDataKeys.Analysis.Issues, new List<ICodeIssue>());
            }

            ((List<ICodeIssue>)node.MetaData[MetaDataKeys.Analysis.Issues]).AddRange(analysisResults);
        }

        void AttachResultsToElement(XmlAttribute attribute, List<ICodeIssue> analysisResults)
        {
            if (!attribute.HasKey(MetaDataKeys.Analysis.Issues))
            {
                attribute.Add(MetaDataKeys.Analysis.Issues, new List<ICodeIssue>());
            }

            ((List<ICodeIssue>)attribute.MetaData[MetaDataKeys.Analysis.Issues]).AddRange(analysisResults);
        }
    }
}

