using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
    class FixMalformedColorUsingColorEditor : FixCodeAction
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public FixMalformedColorUsingColorEditor(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public override Type TargetCodeAnalyser => typeof(MalformedHexadecimalColorValue);

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        public override string Identifier => "com.mfractor.code_fixes.xaml.choose_color_using_editor";

        public override string Name => "Fix Malformed Color Using Color Editor";

        public override string Documentation => "When a hexadecimal color value is invalid, this code fix opens the color picker so you can manually pick the desired color.";

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Choose a color with color picker").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var color = Color.WhiteSmoke;

            return new ColorEditorWorkUnit(color, (result) =>
            {
                var content = result.GetHexString(true);

                if (result.IsNamedColor)
                {
                    content = result.Name;
                }

                WorkEngine.ApplyAsync(new ReplaceTextWorkUnit(document.FilePath, content, syntax.Value.Span)).ConfigureAwait(false);
            }).AsList();
        }
    }
}
