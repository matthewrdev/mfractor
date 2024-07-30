using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.CodeBehindField
{
    class EmptyCodeBehindFieldDeclarationAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Inspects occurances of the `x:Name` attribute and validates that a value is assigned; empty `x:Name` expressions generate an empty named code-behind field, causing compilation errors.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.empty_code_behind_field_name";

        public override string Name => "Empty Code Behind Field Declaration";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1004";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (syntax.HasValue)
			{
                return null;
			}

            if (!XamlSyntaxHelper.IsCodeBehindFieldName(syntax, context.Namespaces))
            {
                return null;
			}

            return CreateIssue($"Empty code-behind field declaration", syntax, syntax.Span).AsList();
		}
	}
}
