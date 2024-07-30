using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Setter;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeActions.Fix.Setters
{
    class FixSetterPropertyName: FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(SetterPropertyExists);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.fix_setter_property_name";

        public override string Name => "Fix Setter Property Name";

        public override string Documentation => "Looks for members on a Setters TargetType that are named closely to the unresolved property name and then suggest near matches.";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return issue.GetAdditionalContent<IPropertySymbol>() != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var property = issue.GetAdditionalContent<IPropertySymbol>();

            return CreateSuggestion("Replace with " + property.Name).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new ReplaceTextWorkUnit()
            {
                FilePath = document.FilePath,
                Text = issue.GetAdditionalContent<IPropertySymbol>().Name,
                Span = issue.Span,
            }.AsList();
        }
    }
}