using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Analysis;
using MFractor.Maui.Symbols;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class PropertySetterNodeTypeMismatch : XamlCodeAnalyser
	{
        public override string Documentation => "When using MyClass.MyProperty node setter syntax, validate that the inner child node returns a .NET object of the correct type for the property.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.property_setter_node_type_mistmatch";

        public override string Name => "Property Setter Type Mismatch";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1041";

		[Import]
		public IXamlTypeResolver XamlTypeResolver { get; set; }

		public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.PropertySetterNodeExecutionFilter;

		protected override IReadOnlyList<ICodeIssue> Analyse (XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (syntax.IsRoot || syntax.IsLeaf)
			{
				return null;
			}

			if (!XamlSyntaxHelper.IsPropertySetter(syntax))
			{
				return null;
			}

			if (!XamlSyntaxHelper.ExplodePropertySetter(syntax, out var className, out var propertyName))
			{
				return null;
			}

			var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol == null)
			{
				return null; //
			}

			if (FormsSymbolHelper.HasTypeConverterAttribute(symbol, context.Platform))
			{
				return null; // Value converter in play, not relevant.
			}

            if (!syntax.HasChildren)
            {
                return null;
            }

			if (syntax.Children.Count > 1)
			{
				return null; // Not relevant, can only inspect 1 child.
			}

			var child = syntax.Children[0];

			var childSymbol = context.XamlSemanticModel.GetSymbol(child) as INamedTypeSymbol;
			if (childSymbol == null
				|| childSymbol.IsGenericType)
			{
				return null;
			}

			var memberType = symbol is IPropertySymbol ? (symbol as IPropertySymbol).Type : (symbol as IFieldSymbol).Type;
            if (memberType is ITypeParameterSymbol)
            {
                var typeArgsAttr = syntax.Parent.GetAttribute((arg) => XamlSyntaxHelper.IsTypeArguments(arg, context.Namespaces));

                if (!typeArgsAttr.HasValue)
                {
                    return null;
                }

                if (!XamlSyntaxHelper.ExplodeTypeReference(typeArgsAttr.Value.Value, out var typeNamespace, out className))
				{
					return null;
				}

                var xamlNamespace = context.Namespaces.ResolveNamespace(typeNamespace);

				memberType = XamlTypeResolver.ResolveType(className, xamlNamespace, context.Project, context.XmlnsDefinitions);
            }

			if (SymbolHelper.DerivesFrom(childSymbol, memberType))
			{
				return null;
			}

			if (SymbolHelper.DerivesFrom(memberType, "System.Collections.IEnumerable"))
			{
				return null; // Collection initiliaser
			}

			if (SymbolHelper.DerivesFrom(childSymbol, context.Platform.MarkupExtension.MetaType))
			{
				return null; // Mark up extension.
			}

			return CreateIssue($"{syntax.Name.LocalName} expects a {memberType} but a {childSymbol} is being provided instead", syntax, child.NameSpan).AsList();
		}
	}
}
