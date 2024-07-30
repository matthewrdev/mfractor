using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class PropertySetterNodeDoesNotExistInParent : XamlCodeAnalyser
    {
        public override string Documentation => "Checks that a property node resolves to a member within its parent type.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.property_setter_node_does_not_exist_in_parent";

        public override string Name => "Property Node Maps To Member In Parent Type";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1039";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.PropertySetterNodeExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol != null)
            {
                return null; //
            }

            if (syntax.Parent == null)
            {
                return null;
            }

            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;
            if (parentSymbol == null)
            {
                return null; // Can't check if there is no parent symbol.
            }

            if (syntax.Name.LocalName == "xmlns"
                || syntax.Name.Namespace == "xmlns")
            {
                return null;
            }

            var elementNamespace = context.Namespaces.ResolveNamespace(syntax);
            if ( elementNamespace == null
                || XamlSchemaHelper.IsMicrosoftSchema(elementNamespace))
            {
                return null;
            }

            if (!XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return null;
            }

            var xamlNamespace = context.Namespaces.ResolveNamespace(syntax.Parent);

            if (xamlNamespace == null
                || XamlSchemaHelper.IsMicrosoftSchema(xamlNamespace))
            {
                return null;
            }

            if (!XamlSyntaxHelper.ExplodePropertySetter(syntax, out var className, out var propertyName))
            {
                return null;
            }

            var xmlns = syntax.Name.HasNamespace ? (syntax.Name.Namespace + ":") : "";
            var namespacedClassName = $"{xmlns}{className}";

            if (syntax.Parent.Name.FullName != namespacedClassName)
            {
                return null; // One is missing a namespace element.
            }

            ISymbol nearestSymbol = null;
            if (parentSymbol != null)
            {
                nearestSymbol = SymbolHelper.ResolveNearestNamedMember(parentSymbol, propertyName);
            }

            var message = $"{syntax.Parent.Name.LocalName} does not have a member named '{syntax.Name.LocalName}'.";
            if (nearestSymbol != null)
            {
                message += $"\n\nDid you mean '{namespacedClassName}.{nearestSymbol.Name}'?";
            }

            return CreateIssue(message, syntax, syntax.NameSpan, nearestSymbol).AsList();
        }
    }
}
