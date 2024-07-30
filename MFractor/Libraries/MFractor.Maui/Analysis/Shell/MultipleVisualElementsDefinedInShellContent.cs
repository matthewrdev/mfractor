using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Shell
{
    class MultipleVisualElementsDefinedInShellContent : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.multiple_visual_elements_defined_in_shell_content";

        public override string Name => "Multiple VisualElements Defined In Shell Content";

        public override string Documentation => "Inspects usages of the ShellContent and validates that only one page or view is declared within it.";

        public override string DiagnosticId => "MF1052";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.Shell.MetaType))
            {
                return Array.Empty<ICodeIssue>();
            }

            var elements = syntax.GetChildren(c =>
            {

                var pageType = context.XamlSemanticModel.GetSymbol(c) as INamedTypeSymbol;

                return SymbolHelper.DerivesFrom(pageType, context.Platform.Page.MetaType) || SymbolHelper.DerivesFrom(pageType, context.Platform.View.MetaType);
            });

            if (elements.Count <= 1)
            {
                return Array.Empty<ICodeIssue>();
            }

            return elements.Select(el => CreateIssue("Multiple elements are declared inside this shell content; only the last element will be rendered. Is this intended?", el, el.OpeningTagSpan)).ToList();
        }
    }
}
