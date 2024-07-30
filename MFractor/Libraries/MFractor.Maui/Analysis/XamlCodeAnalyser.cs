using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Xml;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A code analyser that inspects XAML documents.
    /// </summary>
    public abstract class XamlCodeAnalyser : CodeAnalyser
    {
        /// <summary>
        /// A filter to quickly include or exclude this analyser analysing particular file types.
        /// </summary>
        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.XamlDocumentFilter;

        /// <summary>
        /// Can this <paramref name="document"/> and <paramref name="context"/> be analysed?
        /// </summary>
        public sealed override bool CanAnalyseDocument(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return false;
            }

            return IsInterestedInXamlDocument(xamlDocument, xamlContext);
        }

        /// <summary>
        /// Is this code analyser interested in this particular XAML <paramref name="document"/>?
        /// </summary>
        protected virtual bool IsInterestedInXamlDocument(IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return true;
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        protected sealed override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            return Analyse(syntax, xamlDocument, xamlContext);
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        protected virtual IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            return Array.Empty<ICodeIssue>();
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        protected sealed override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax,
                                                               IParsedXmlDocument document,
                                                               IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            return Analyse(syntax, xamlDocument, xamlContext);
        }

        /// <summary>
        /// Analyse the specified syntax, document and context.
        /// </summary>
        protected virtual IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context)
        {
            return Array.Empty<ICodeIssue>();
        }
    }
}
