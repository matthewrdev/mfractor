using System;
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
    class OnPlatformReturnTypeMistmatchAnalysis : XamlCodeAnalyser
	{
        public override string Documentation => "Checks the type returned by a `.OnPlatform` element is valid with the parent property type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.on_platform_return_type_mismatch";

        public override string Name => "OnPlatform Return Type Mismatch";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1037";

		readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
		public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

		[ImportingConstructor]
		public OnPlatformReturnTypeMistmatchAnalysis(Lazy<IXamlTypeResolver> xamlTypeResolver)
		{
			this.xamlTypeResolver = xamlTypeResolver;
		}

		protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (symbol == null
				|| !context.Platform.SupportsOnPlatform
				|| !symbol.Name.StartsWith(context.Platform.OnPlatform.NonGenericName))
			{
				return null;
			}

            if (!(symbol.ContainingNamespace.ToString() == context.Platform.OnPlatform.Namespace))
			{
				return null;
			}

			if (syntax.Parent == null)
			{
				return null;
			}

            var typeArgsAttr = syntax.GetAttribute(attr => XamlSyntaxHelper.IsTypeArguments(attr, context.Namespaces));

			if (typeArgsAttr == null
                || !typeArgsAttr.HasValue)
			{
				return null;
			}

			ITypeSymbol expectedType = null;
			if (XamlSyntaxHelper.IsPropertySetter(syntax.Parent))
			{
				var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent);
                if (parentSymbol == null)
				{
					return null; // Parent symbol needs to be valid.
				}

				var memberType = parentSymbol is IPropertySymbol ? (parentSymbol as IPropertySymbol).Type : (parentSymbol as IFieldSymbol).Type;
				if (memberType == null)
				{
					return null;
				}

				expectedType = memberType;
			}
			else 
            {
				if (syntax.Parent.Parent == null)
				{
					return null;
				}

				var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent.Parent) as INamedTypeSymbol;

                if (!SymbolHelper.DerivesFrom(parentSymbol,context.Platform.ResourceDictionary.MetaType))
				{
					return null;
				}

				parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
                if (parentSymbol == null)
				{
					return null; // Parent symbol needs to be valid.
				}

				expectedType = parentSymbol;
			}

            if (!XamlSyntaxHelper.ExplodeTypeReference(typeArgsAttr.Value.Value, out var typeNamespace, out var className))
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(typeNamespace);

			var innerType = XamlTypeResolver.ResolveType(className, xamlNamespace, context.Project, context.XmlnsDefinitions);
			if (innerType == null)
			{
				return null;
			}

            if (SymbolHelper.DerivesFrom(innerType, expectedType))
			{
				return null;
			}

            if (SymbolHelper.DerivesFrom(expectedType,context.Platform.ResourceDictionary.MetaType))
            {
                return null;
            }

            return CreateIssue($"This OnPlatform element returns a {innerType} but the outer element {syntax.Parent.Name.FullName} expects a {expectedType.ToString()}", syntax, syntax.NameSpan).AsList();
		}
	}
}
