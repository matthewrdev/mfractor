using System;
using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Refactor.Colors
{
    class AddTransparencyChannelToColor : RefactorXamlCodeAction
    {
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.add_transparency_to_color";

        public override string Name => "Add Transparency Channel To Color";

        public override string Documentation => "Given a color literal defined by a hex value, this code action adds a transparency channel to that color.";

        public override string AnalyticsEvent => Name;

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            if (symbol == null || ExpressionParserHelper.IsExpression(syntax.Value?.Value))
            {
                return false;
            }

            if (!XamlSyntaxHelper.IsColorSymbol(syntax, context.XamlSemanticModel, context.Platform))
            {
                return false;
            }

            var value = syntax.Value.Value;

            if (!value.StartsWith("#", StringComparison.Ordinal))
            {
                return false;
            }

            // #RGB | #RRGGBB
            if (value.Length == 4 || value.Length == 7)
            {
                return true;
            }

            return false;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Add a transparency channel to this color.").AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var value = syntax.Value.Value;

            if (value.Length == 4)
            {
                value = value.Insert(1, "0");
            }
            else if (value.Length == 7)
            {
                value = value.Insert(1, "00");
            }

            return new ReplaceTextWorkUnit()
            {
                FilePath = document.FilePath,
                Span = syntax.Value.Span,
                Text = value
            }.AsList();
        }
    }
}
