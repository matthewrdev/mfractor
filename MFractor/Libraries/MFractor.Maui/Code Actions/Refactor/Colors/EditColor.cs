using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Refactor.Colors
{
    class EditColor : RefactorXamlCodeAction
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine  WorkEngine => workEngine.Value;

        public override string Documentation => "Use the Edit Color code action to visually edit a color from your Xaml.";

        public override string Identifier => "com.mfractor.code_actions.xaml.edit_color";

        public override string Name => "Edit Color Declaration";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        [ImportingConstructor]
        public EditColor(Lazy<IWorkEngine> workEngine)
        {
            this.workEngine = workEngine;
        }

        public override bool CanExecute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (symbol == null || expression != null || !syntax.HasValue)
            {
                return false;
            }

            if (!XamlSyntaxHelper.IsColorSymbol(syntax, context.XamlSemanticModel, context.Platform))
            {
                return false;
            }

            Color? color = null;
            try
            {
                color = ColorTranslator.FromHtml(syntax.Value.Value);
            }
            catch { }

            return color.HasValue;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Edit Color", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var input = syntax.Value.Value.Split('.').Last();

            Color? color = null;
            try
            {
                color = ColorTranslator.FromHtml(syntax.Value.Value);
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            {
            }

            void handleInput(Color colorSelection)
            {
                var text = GetHexString(colorSelection);
                if (colorSelection.IsNamedColor)
                {
                    text = colorSelection.Name;
                }

                var workUnit = new ReplaceTextWorkUnit()
                {
                    FilePath = document.FilePath,
                    Span = syntax.Value.Span,
                    Text = text,
                };

                WorkEngine.ApplyAsync(workUnit);
            }

            return new ColorEditorWorkUnit(color.Value, handleInput).AsList();
        }

        public string GetHexString(Color color)
        {
            return ColorHelper.GetHexString(color, true);
        }
    }
}
