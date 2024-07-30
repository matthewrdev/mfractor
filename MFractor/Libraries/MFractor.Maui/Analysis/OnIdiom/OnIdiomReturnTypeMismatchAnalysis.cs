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
    class OnIdiomReturnTypeMismatchAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "Checks the type returned by a `.OnIdiom` element is valid with the parent property type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.on_idiom_return_type_mismatch";

        public override string Name => "OnIdiom Return Type Mismatch";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1036";

        [Import]
        public IXamlTypeResolver XamlTypeResolver { get; set; }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
            if (symbol == null
                || !context.Platform.SupportsOnIdiom
                || !symbol.Name.StartsWith(context.Platform.OnIdiom.NonGenericName))
            {
                return null;
            }

            if (!(symbol.ContainingNamespace.ToString() == context.Platform.OnIdiom.Namespace))
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

            return CreateIssue($"This OnIdiom element returns a {innerType.ToString()} but the outer element {syntax.Parent.Name.FullName} expects a {expectedType.ToString()}", syntax, syntax.NameSpan).AsList();
        }
    }
}
