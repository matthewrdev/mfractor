using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class PropertySetterAttributeDoesNotExistInParentAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks that an attribute resolves to a member within its parent type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.property_setter_attribute_does_not_exist_in_parent";

        public override string Name => "Referenced Attribute Member Exists In Parent Type";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1038";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (!context.Platform.SupportsStyleSheets)
			{
				return null;
			}

			var symbol = context.XamlSemanticModel.GetSymbol(syntax);
			if (symbol != null)
			{
				return null;
			}

			var parentSyntax = syntax.Parent;
			var parentSyntaxNamespace = context.Namespaces.ResolveNamespace(parentSyntax);
			if (XamlSchemaHelper.IsMicrosoftSchema(parentSyntaxNamespace))
			{
				return null;
			}

			var parentSymbol = context.XamlSemanticModel.GetSymbol(parentSyntax) as ITypeSymbol;
			if (parentSymbol == null)
			{
				return null; // Can't check if there is no parent symbol.
			}

            // Explicitly filter out style sheets.
            if (SymbolHelper.DerivesFrom(parentSymbol, context.Platform.StyleSheet.MetaType) 
                && string.Equals(syntax.Name.LocalName, "Source", System.StringComparison.InvariantCulture))
            {
                return null;
            }

			if ( syntax.Name.LocalName == "xmlns"
			    || syntax.Name.Namespace == "xmlns")
			{
				return null;
			}

            var elementNamespace = context.Namespaces.ResolveNamespace(syntax);

			if (elementNamespace == null)
			{
				return null;
			}

			if (XamlSchemaHelper.IsMicrosoftSchema(elementNamespace))
			{
				return null;
			}

			var isDesign = false;
			if (XamlSchemaHelper.IsDesign(elementNamespace))
			{
				isDesign = true;
				elementNamespace = context.Namespaces.DefaultNamespace;
			}

			// Check if it is an attached property.
			if (syntax.Name.LocalName.Contains("."))
			{
				return null; // Don't analyse attached properties, we only care about member properties on for the parent node.
			}

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax.Name.Namespace);
			if (xamlNamespace == null)
			{
				return null;
			}

			if (XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace) || XamlSchemaHelper.IsMarkupCompatibilitySchema(xamlNamespace))
			{
				return null;
			}

			ISymbol nearestSymbol = null;
			if (parentSymbol != null)
			{
				nearestSymbol = SymbolHelper.ResolveNearestNamedMember(parentSymbol, syntax.Name.LocalName);
			}

			PropertySetterAttributeDoesNotExistInParentBundle bundle = null;

			var message = $"{parentSymbol} does not have a member named '{syntax.Name.LocalName}'.";
			if (nearestSymbol != null)
			{
				var designXmlns = context.Namespaces.ResolveNamespaceForSchema(XamlSchemas.DesignSchemaUrl);

				var suggestion = isDesign && designXmlns != null ? designXmlns.Prefix + ":" + nearestSymbol.Name : nearestSymbol.Name;

				bundle = new PropertySetterAttributeDoesNotExistInParentBundle(suggestion);

				message += $"\n\nDid you mean '{suggestion}'?";
			}

            return CreateIssue(message, syntax, syntax.NameSpan, bundle).AsList();
		}
	}
}

