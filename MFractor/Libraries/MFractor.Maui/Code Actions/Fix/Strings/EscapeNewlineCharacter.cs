using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Strings;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Strings
{
    class EscapeNewlineCharacter : FixCodeAction
    {
        public override string Documentation => "When the '\\n' newline character is used within a XAML attributes string, it will render as '\\n' rather than create a newline. This code fix replaces C# style newlines with the escaped &#10; character code.";

        public override Type TargetCodeAnalyser => typeof(UnescapedNewlineInStringLiteral);

        public override string Identifier => "com.mfractor.code_fixes.xaml.escape_newline_character";

        public override string Name => "Escape Newline Characters";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var microsoftNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

            if (microsoftNamespace == null)
            {
                return false;
            }

            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Escape '\\n' newlines using '&#10;'").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var newContent = syntax.Value.Value.Replace("\\n", "&#10;");

            return new ReplaceTextWorkUnit(document.FilePath, newContent, syntax.Value.Span).AsList();
        }
    }
}
