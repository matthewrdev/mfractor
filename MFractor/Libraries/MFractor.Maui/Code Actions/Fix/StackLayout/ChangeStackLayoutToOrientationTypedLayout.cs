using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.StackLayout;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.StackLayout
{
    class ChangeStackLayoutToOrientationTypedLayout : FixCodeAction
    {
        public override Type TargetCodeAnalyser => typeof(StackLayoutCanBeChangedToTypedOrientation);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        public override string Identifier => "com.mfractor.code_fixes.xaml.change_stack_layout_to_orientation_typed_layout";

        public override string Name => "Change StackLayout To Orientation Typed Layout";

        public override string Documentation => "";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        public enum Conversion
        {
            Vertical,

            Horizontal,
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var orientation = syntax.GetAttributeByName(context.Platform.OrientationProperty);
            var orientationValue = orientation.Value.Value;
            if (orientationValue == context.Platform.StackLayoutOrientation_Vertical)
            {
                return CreateSuggestion($"Convert to {context.Platform.VerticalStackLayout}", Conversion.Vertical).AsList();
            }
            else if (orientationValue == context.Platform.StackLayoutOrientation_Horizontal)
            {
                return CreateSuggestion($"Convert to {context.Platform.HorizontalStackLayout}", Conversion.Horizontal).AsList();
            }

            return null;
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var conversion = suggestion.GetAction<Conversion>() == Conversion.Vertical ? context.Platform.VerticalStackLayout.Name : context.Platform.HorizontalStackLayout.Name;
            var orientation = syntax.GetAttributeByName(context.Platform.OrientationProperty);

            return new List<IWorkUnit>()
            {
                new DeleteXmlSyntaxWorkUnit()
                {
                    FilePath = document.FilePath,
                    Syntaxes = orientation.AsList()
                },
                new ReplaceTextWorkUnit(document.FilePath, conversion, syntax.NameSpan)
            };
        }
    }
}
