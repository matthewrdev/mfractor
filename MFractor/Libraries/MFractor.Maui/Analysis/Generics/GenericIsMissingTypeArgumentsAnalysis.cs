using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Generics
{
    class GenericIsMissingTypeArgumentsAnalysis: XamlCodeAnalyser
	{
        public override string Documentation => "Inspects generic classes that are instantiated through Xaml and validates that an `x:TypeArguments` attribute or property assignment node is present.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.generic_is_missing_type_arguments";

        public override string Name => "Generic Usage Is Missing x:TypeArguments";

        public override string DiagnosticId => "MF1021";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

		protected override IReadOnlyList<ICodeIssue> Analyse (XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var microsoftXamlNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);
			var namedType = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (namedType == null
                || !namedType.IsGenericType
                || microsoftXamlNamespace == null)
			{
                return null;
			}

			if (syntax.HasAttribute((arg) => arg.Name.Namespace == microsoftXamlNamespace.Prefix && arg.Name.LocalName == Keywords.MicrosoftSchema.TypeArguments))
            {
                return null;
			}

			if (syntax.HasChild((arg) => arg.Name.Namespace == microsoftXamlNamespace.Prefix 
                                && arg.Name.LocalName == Keywords.MicrosoftSchema.TypeArguments))
            {
                return null;
			}

            return CreateIssue($"The generic type {namedType} has no type arguments supplied. Use x:TypeArguments to specify type arguments.", syntax, syntax.NameSpan).AsList();
        }
	}
}
