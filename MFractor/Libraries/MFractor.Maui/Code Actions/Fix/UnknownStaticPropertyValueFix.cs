using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix
{
    class UnknownStaticPropertyValueFix : FixCodeAction
    {
        public override string Documentation => "When a xaml element property attempts to reference a static field (for instance LayoutOptions.Center), this fix finds the nearest named member and replaces the incorrect value with an auto-correction.";

        public override Type TargetCodeAnalyser => typeof(UnknownStaticPropertyValue);

        public override string Identifier => "com.mfractor.code_fixes.xaml.unknown_static_property_value";

        public override string Name => "Replace Unknown Property Value With Autocorrection";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            if (!syntax.HasValue)
            {
                return false;
            }

            var symbol = issue.GetAdditionalContent<ISymbol>();

            return symbol != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<ISymbol>();
            var newName = GenerateValueName(syntax.Value.Value, symbol);
            if (string.IsNullOrEmpty(newName))
            {
                return null;
            }

            return CreateSuggestion("Replace with '" + newName + "'", 0).AsList();
        }

        public string GenerateValueName(string elementValue, ISymbol symbol)
        {
            var prepend = "";
            var newName = elementValue;
            if (newName.Contains("."))
            {
                var components = newName.Split('.');
                if (components.Length == 2)
                {
                    prepend = components[0] + ".";
                }
                else
                {
                    return null;
                }
            }

            return prepend + symbol.Name;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var symbol = issue.GetAdditionalContent<ISymbol>();
            var newName = GenerateValueName(syntax.Value.Value, symbol);
            return new ReplaceTextWorkUnit(document.FilePath, newName, syntax.Value.Span).AsList();
        }
    }
}

