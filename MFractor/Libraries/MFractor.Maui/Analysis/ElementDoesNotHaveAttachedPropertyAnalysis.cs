using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Symbols;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
	class ElementDoesNotHaveAttachedPropertyAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Looks for attached properties (for example `Grid.Row`) and validates they exist in the class that they are attempting to use.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.element_does_not_have_attached_property";

        public override string Name => "Class Does Not Have Attached Property";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1069";

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        [ImportingConstructor]
        public ElementDoesNotHaveAttachedPropertyAnalysis(Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.xamlTypeResolver = xamlTypeResolver;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (syntax.Name.LocalName.Contains(".") == false)
            {
                return null;
            }

			var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol != null)
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax);
			if (xamlNamespace == null)
            {
                return null;
            }

			// Resolve the base class.
			var propertyName = syntax.Name.LocalName.Split('.').Last() + "Property";
			var typeName = syntax.Name.LocalName.Split('.').First();

            var parentSymbol = XamlTypeResolver.ResolveType(typeName, xamlNamespace, context.Project, context.XmlnsDefinitions);

			if (parentSymbol == null)
			{
                return null;
			}

			var message = $"{parentSymbol.ToString()} does not contain an attached property named '{propertyName}'.";

			ISymbol nearestSymbol = null;
			if (parentSymbol != null)
			{
				nearestSymbol = SymbolHelper.ResolveNearestNamedMember(parentSymbol, propertyName);
			}

			if (nearestSymbol != null)
			{
				message += $"\n\nDid you mean '{parentSymbol.Name + "." + nearestSymbol.Name.Substring(0, nearestSymbol.Name.LastIndexOf("Property", StringComparison.Ordinal))}'?";
			}

            return CreateIssue(message, syntax, syntax.NameSpan, nearestSymbol).AsList();
        }
	}
}

