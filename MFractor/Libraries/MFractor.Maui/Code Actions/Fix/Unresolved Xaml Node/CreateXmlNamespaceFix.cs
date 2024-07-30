using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.UnresolvedXamlNode
{
    class CreateXmlNamespaceFix : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(Analysis.XamlNodeDoesNotResolveAnalysis);

        public override string Documentation => "The Create Xml Namespace For Symbol let's you import a new XML namespace for a XAML symbol.";

        public override string Identifier => "com.mfractor.code_fixes.xaml.create_xml_namespace";

        public override string Name => "Create Xml Namespace For Symbol";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator { get; set; }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbols = issue.GetAdditionalContent<List<INamedTypeSymbol>>();

            return symbols != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbols = issue.GetAdditionalContent<List<INamedTypeSymbol>>();

            var suggestions = new List<ICodeActionSuggestion>();
            for (var i = 0; i < symbols.Count; ++i)
            {
                var symbol = symbols[i];
                suggestions.Add(CreateSuggestion($"Import '{symbol.Name}' from '{symbol.ContainingNamespace.ToString()}'", i));
            }

            return suggestions;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbols = issue.GetAdditionalContent<List<INamedTypeSymbol>>();

            var symbol = symbols[suggestion.ActionId];

            var xmlns = symbol.ContainingNamespace.Name.ToLower();

            var xmlPolicy = XmlFormattingPolicyService.GetXmlFormattingPolicy();

            var workUnits = XamlNamespaceImportGenerator.CreateXmlnsImportStatementWorkUnit(document, context.Platform, xmlns, symbol, context.Project, xmlPolicy).ToList();

            var replace = new ReplaceTextWorkUnit
            {
                Text = xmlns + ":" + syntax.Name,
                FilePath = document.FilePath,
                Span = syntax.NameSpan
            };

            workUnits.Add(replace);

            return workUnits;
        }
    }
}

