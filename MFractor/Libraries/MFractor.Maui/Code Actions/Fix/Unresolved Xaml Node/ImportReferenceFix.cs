using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.XamlNamespaces;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.UnresolvedXamlNode
{
    class ImportReferenceFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(XmlNamespaceDoesNotExistAnalysis);

        public override string Documentation => "Use the Import Reference fix to generate an xmlns namespace import for a missing control in your XAML.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.import_reference";

        public override string Name => "Import Namespace And Assembly For Unresolved XAML Node";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();

            return symbol != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();

            return CreateSuggestion($"Import '{symbol.ContainingNamespace.ToString()}' as xmlns:{syntax.Name.Namespace}", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<INamedTypeSymbol>();

            var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            return XamlNamespaceImportGenerator.CreateXmlnsImportStatementWorkUnit(document, context.Platform, syntax.Name.Namespace, symbol, context.Project, xmlPolicy);
        }
    }
}

