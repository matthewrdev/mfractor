using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Schemas
{
	class MissingMicrosoftXamlSchema : XamlCodeAnalyser
	{
        public override string Documentation => "Inspects at the root xaml node and verifies that it references the Microsoft Xaml schema: `http://schemas.microsoft.com/winfx/2009/xaml`. This schema is required for Xamarin Forms Xaml documents.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.missing_microsoft_schema_reference";

        public override string Name => "Missing Microsoft Schema";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1046";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (!syntax.IsRoot)
			{
                return null;
			}

            if (syntax.HasAttribute(attr => attr.Value?.Value == XamlSchemas.MicrosoftSchemaUrl))
            {
                return null;
            }

            return CreateIssue(syntax.Name + " is missing the Microsoft Xaml schema namespace",
                                     syntax,
                                     syntax.NameSpan).AsList();
        }
	}
}
