using System;
using System.Collections.Generic;
using System.Drawing;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Colors;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Colors
{
    class ReplaceHexValueWithNamedColor : FixCodeAction
    {
        public IReadOnlyDictionary<string, Color> Colors { get; } = ColorHelper.GetAllSystemDrawingColors();

        public override Type TargetCodeAnalyser => typeof(HexadecimalValueMatchesNamedColor);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.replace_hex_value_with_named_color";

        public override string Name => "Replace Hexadecimal Value With Named Color";

        public override string Documentation => "When a hexadecimal color value is used that matches a named color, this code fix replaces the hex value with the named color";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            ColorHelper.GetMatchingColor(syntax.Value.Value, out var color);

            return CreateSuggestion("Replace with " + color.Name).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            ColorHelper.GetMatchingColor(syntax.Value.Value, out var color);

            return new ReplaceTextWorkUnit(document.FilePath, color.Name, syntax.Value.Span).AsList();
        }
    }
}
