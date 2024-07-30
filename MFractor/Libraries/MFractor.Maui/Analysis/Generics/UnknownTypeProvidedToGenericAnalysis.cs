using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Symbols;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Generics
{
    class UnknownTypeProvidedToGenericAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Inspects usages of `x:TypeArguments` and validates that the type provided exists.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.unknown_type_provided_to_generic";

        public override string Name => "Unknown Type Provided To Generic";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1023";

        [Import]
        public IXamlTypeResolver XamlTypeResolver { get; set; }

        [Import]
        public IXmlnsNamespaceSymbolResolver XmlnsNamespaceSymbolResolver { get; set; }

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
            var microsoftXamlNamespace = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);

            if (microsoftXamlNamespace is null)
            {
                return null;
            }

			if (syntax.Name.Namespace != microsoftXamlNamespace.Prefix
                || syntax.Name.LocalName != Keywords.MicrosoftSchema.TypeArguments
                || syntax.HasValue == false)
            {
                return null;
			}

            if (!XamlSyntaxHelper.ExplodeTypeReference(syntax.Value.Value, out var typeNamespace, out var className))
            {
                return null;
			}

            var xamlNamespace = context.Namespaces.ResolveNamespace(typeNamespace);
			if (xamlNamespace == null)
            {
                return null;
			}

            var elementType = XamlTypeResolver.ResolveType(className, xamlNamespace, context.Project, context.XmlnsDefinitions);
			if (elementType != null)
            {
                return null;
			}

            var assemblies = XmlnsNamespaceSymbolResolver.GetAssemblies(xamlNamespace, context.Project, context.XmlnsDefinitions);
            var namespaces = XmlnsNamespaceSymbolResolver.GetNamespaces(xamlNamespace, context.Project, context.XmlnsDefinitions)?.ToList();
            if (assemblies is null || !assemblies.Any() || namespaces is null || !namespaces.Any())
            {
                return null;
            }

            // Locate the namespace and assembly, perform a fuzzy search for a similiarly named symbol.
            var similiarSymbol = FormsSymbolHelper.ResolveNearlyNamedTypeFromAssemblies(assemblies, className, context.Compilation);
			if (similiarSymbol != null) 
            {
                return null;
			}

            var namespaceValue = namespaces.Count > 1 ? xamlNamespace.Value : namespaces.First().ToString();

            var message = $"{namespaceValue} does not contain a class or struct named '{className}'.";

			if (similiarSymbol != null)
			{
				message += $"\n\nDid you mean '{similiarSymbol.Name}'?";
			}

            return CreateIssue(message, syntax, syntax.Value.Span, similiarSymbol).AsList();
        }
	}
}
