using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Shell
{
    class ShellRequiresAtLeastOneItem : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.shell_requires_at_least_one_shell_item";

        public override string Name => "Shell Requires At Least One Item";

        public override string Documentation => "When using `.Shell` you must provide at least one item declaration.";

        public override string DiagnosticId => "MF1053";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!syntax.IsRoot)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.Shell.MetaType))
            {
                return null;
            }

            var itemsSetter = syntax.GetChildNode(c =>
            {
                var property = context.XamlSemanticModel.GetSymbol(c) as IPropertySymbol;

                return property?.Name == "Items";
            });

            var candidate = itemsSetter ?? syntax;

            var elements = candidate.GetChildren(c =>
            {
                var pageType = context.XamlSemanticModel.GetSymbol(c) as INamedTypeSymbol;

                return SymbolHelper.DerivesFrom(pageType, context.Platform.ShellItem.MetaType);
            });

            if (elements.Any())
            {
                return null;
            }

            return CreateIssue("You must provide at least one item to this Shell such as a FlyoutItem or TabBar.", syntax, syntax.NameSpan).AsList();
        }
    }
}
