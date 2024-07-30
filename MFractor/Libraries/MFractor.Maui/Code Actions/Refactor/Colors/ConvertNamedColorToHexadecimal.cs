using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Code.CodeActions;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using MFractor.Work;

namespace MFractor.Maui.CodeActions.Refactor.Colors
{
    class ConvertNamedColorToHexadecimal : RefactorXamlCodeAction
    {
        public enum ConversionType
        {
            WithTransparency,
            WithoutTransparency,
        }

        readonly IReadOnlyDictionary<string, Color> colorMap = ColorHelper.GetAllSystemDrawingColors();

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_actions.xaml.convert_named_color_to_hexadecimal";

        public override string Name => "Convert Named Color To Hexadecimal";

        public override string Documentation => "Given a color that is defined by a named color literal (such as Red, Green etc), this code action converts that ";

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

            return colorMap.ContainsKey(syntax.Value.Value);
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return new List<ICodeActionSuggestion>()
            {
                CreateSuggestion("Convert color to hex format", ConversionType.WithoutTransparency),
                CreateSuggestion("Convert color to hex format with an alpha channel", ConversionType.WithoutTransparency),
            };
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var value = syntax.Value.Value;

            var color = colorMap[value];

            var conversion = ColorHelper.GetHexString(color.R, color.G, color.B, true);

            if (suggestion.IsAction(ConversionType.WithTransparency))
            {
                conversion = ColorHelper.GetHexString(color.R, color.G, color.B, 0, true);
            }

            return new ReplaceTextWorkUnit()
            {
                FilePath = document.FilePath,
                Span = syntax.Value.Span,
                Text = conversion,
            }.AsList();
        }
    }
}
