using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Documents;
using Microsoft.Language.Xml;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAnalyserSuppressionService))]
    class AnalyserSuppressionService : IAnalyserSuppressionService
    {
        readonly Lazy<ICodeAnalyserRepository> codeAnalysers;
        public ICodeAnalyserRepository CodeAnalysers => codeAnalysers.Value;

        [ImportingConstructor]
        public AnalyserSuppressionService(Lazy<ICodeAnalyserRepository> codeAnalysers)
        {
            this.codeAnalysers = codeAnalysers;
        }

        public IReadOnlyDictionary<string, AnalyserSuppression> GetSuppressedAnalysers(IParsedXmlDocument xmlDocument)
        {
            var suppressions = new Dictionary<string, AnalyserSuppression>();
            if (xmlDocument == null)
            {
                return suppressions;
            }

            var syntaxTree = xmlDocument.GetSyntaxTree();

            if (syntaxTree == null)
            {
                return suppressions;
            }

            var xmlDocumentSyntax = syntaxTree.GetRawSyntax<XmlDocumentSyntax>();

            if (xmlDocumentSyntax == null)
            {
                return suppressions;
            }

            var suppressionComments = GetSuppresionComments(xmlDocumentSyntax.PrecedingMisc);

            if (suppressionComments == null || !suppressionComments.Any())
            {
                return suppressions;
            }

            foreach (var comment in suppressionComments)
            {
                var value = comment.Value;

                var suppression = ParseSuppression(value);

                if (suppression != null)
                {
                    suppressions[suppression.Identifier] = suppression;
                }
            }

            return suppressions;
        }

        List<XmlCommentSyntax> GetSuppresionComments(SyntaxList<SyntaxNode> precedingMisc)
        {
            if (precedingMisc.Count == 0)
            {
                return new List<XmlCommentSyntax>();
            }

            return precedingMisc.OfType<XmlCommentSyntax>().ToList();
        }

        AnalyserSuppression ParseSuppression(string value)
        {
            // Expected format: [MFractor: Suppress(MF1058)]
            // Expected format: [MFractor: Suppress(com.mfractor.identifier)]
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            value = value.Replace("[", string.Empty)
                         .Replace("]", string.Empty)
                         .Trim();

            var split = value.Split(':');

            if (split == null || split.Length < 2)
            {
                return default;
            }

            var idComponent = split[0].Trim();
            var suppressionComponent = split[1].Trim();

            if (!string.Equals("MFractor", idComponent, StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }

            split = suppressionComponent.Split('(');

            if (split == null || split.Length < 2)
            {
                return default;
            }

            var keyword = split[0].Trim();
            var diagnosticId = split[1].Trim();

            if (!string.Equals("Suppress", keyword, StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }

            diagnosticId = diagnosticId.Replace(")", string.Empty).Trim();

            var analyser = CodeAnalysers.GetByDiagnosticId(diagnosticId.ToUpper());

            if (analyser == null)
            {
                analyser = CodeAnalysers.GetByIdentifier(diagnosticId);
            }

            if (analyser == null)
            {
                return default;
            }

            return new AnalyserSuppression(analyser.DiagnosticId, analyser.Identifier);
        }
    }
}