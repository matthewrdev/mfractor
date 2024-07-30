using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Maui.Analysis.Generics;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.CodeActions
{
    class CreateTypeArgumentsAttributeFix : FixCodeAction
    {
        public override string Documentation => "Generates an x:TypeArguments attribute, resolving the potential argument types if possible.";

        public override Type TargetCodeAnalyser => typeof(GenericIsMissingTypeArgumentsAnalysis);

        public override string Identifier => "com.mfractor.code_fixes.xaml.create_type_arguments_attribute";

        public override string Name => "Create x:TypeArguments Attribute";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        [ImportingConstructor]
        public CreateTypeArgumentsAttributeFix(Lazy<IMarkupExpressionEvaluater> expressionEvaluater)
        {
            this.expressionEvaluater = expressionEvaluater;
        }

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion($"Add a TypeArguments attribute", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue,
                                                          XmlNode syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context,
                                                          ICodeActionSuggestion suggestion,
                                                          InteractionLocation location)
        {
            var microsoftSchemaNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

            var schema = string.IsNullOrEmpty(microsoftSchemaNamespace.Prefix) ? "" : $"{microsoftSchemaNamespace.Prefix}";

            var importedType = "";
            var content = $" {schema}:{Keywords.MicrosoftSchema.TypeArguments}=\"{importedType}\"";

            var insertLocation = new TextSpan(syntax.NameSpan.End, 0);

            return new ReplaceTextWorkUnit(document.FilePath, content, insertLocation).AsList();
        }
    }
}
