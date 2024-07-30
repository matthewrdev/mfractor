using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Analysis.Strings
{
    class UnescapedNewlineInStringLiteral : XamlCodeAnalyser
    {
        public override string Documentation => "When a newline character, \\n, is used within a string literal in XAML, it will not render as expected. Newlines should be added to XAML using the escaped &#10; character code.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.unescaped_newline_in_string_literal";

        public override string Name => "Unescaped Newline In String Literal";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1096";

        const string newlineCharacter = "\\n";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);

            if (returnType == null
                || returnType.SpecialType != SpecialType.System_String)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (!syntax.HasValue)
            {
                return Array.Empty<ICodeIssue>();
            }

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var indices = syntax.Value.Value.AllIndexesOf(newlineCharacter);

            if (indices is null || !indices.Any())
            {
                return Array.Empty<ICodeIssue>();
            }

            var startOffset = syntax.Value.Span.Start;
            var length = newlineCharacter.Length;

            return indices.Select(i =>
            {
                var span = TextSpan.FromBounds(startOffset + i, startOffset + i + length);
                return CreateIssue("This newline character is unescaped, use &#10; to represent new lines in XAML.",
                                    syntax,
                                    span);
            }).ToList();
        }
    }
}
