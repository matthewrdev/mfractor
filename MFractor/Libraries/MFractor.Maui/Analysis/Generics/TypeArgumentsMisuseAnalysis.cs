using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Generics
{
    class TypeArgumentsMisuseAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Inspects for usages of `x:TypeArguments` on elements that are non-generic classes.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.type_arguments_misuse";

        public override string Name => "x:TypeArguments Used On Non-Generic Class";

        public override string DiagnosticId => "MF1022";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

		protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
            var microsoftXamlNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

            if (microsoftXamlNamespace is null)
            {
                return null;
            }

			if (syntax.Name.Namespace != microsoftXamlNamespace.Prefix
                || syntax.Name.LocalName != Keywords.MicrosoftSchema.TypeArguments)
            {
                return null;
			}

            var namedType = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (namedType == null
                || namedType.IsGenericType)
            {
                return null;
			}

            return CreateIssue($"{namedType} is not a generic type but x:TypeArguments is being used to specify it as a generic.", syntax, syntax.Span).AsList();
        }
	}
}
