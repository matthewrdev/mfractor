using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.CodeBehindField
{
    class CodeBehindFieldNameContainsInvalidCharactersAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks that x:Name expressions define a valid code-behind variable name. A value declared by an x:Name attribute must start with a @, _ or a-Z character followed by underscores or alpha-numeric characters.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.code_behind_field_name_has_invalid_characters";

        public override string Name => "x:Name Has Invalid Characters";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1002";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);

			if (!XamlSchemaHelper.IsSchema(xamlNamespace, XamlSchemas.MicrosoftSchemaUrl)
                || syntax.Name.LocalName != Keywords.MicrosoftSchema.CodeBehindName)
            {
                return null;
            }

			if (!syntax.HasValue)
            {
                return null;
            }

            var isValid = CSharpNameHelper.IsValidCSharpName(syntax.Value.Value);

			if (isValid)
            {
                return null;
            }

            return CreateIssue($"'{syntax.Value}' contains invalid characters. x:Name values may start with @, _ or a-Z followed by underscores or any alphanumerical character.", syntax, syntax.Value.Span).AsList();
		}
	}
}

