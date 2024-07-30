using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class PropertySetterNodeMisuse : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects for property setters that don't apply to the outer class. For example, if a developer used OnIdiom.Phone inside a OnPlatform element, the OnIdiom.Phone property setter makes no sense within the given context.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.property_setter_node_misuse";

        public override string Name => "Property Setter Node Misuse";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1040";

        public override CodeAnalyserExecutionFilter Filter => XamlCodeAnalysisExecutionFilters.PropertySetterNodeExecutionFilter;

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.IsRoot)
            {
                return null;
            }

            if (!XamlSyntaxHelper.IsPropertySetter(syntax))
            {
                return null;
            }

            var elementSymbol = context.XamlSemanticModel.GetSymbol(syntax) as ISymbol;
            var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ISymbol;

            if (elementSymbol == null || parentSymbol == null)
            {
                return null;
            }

            var elementType = elementSymbol is IPropertySymbol ? (elementSymbol as IPropertySymbol).ContainingType : (elementSymbol as IFieldSymbol).ContainingType;
            var parentType = parentSymbol as INamedTypeSymbol;

            if (elementSymbol is IFieldSymbol && elementSymbol.Name.EndsWith("Property", StringComparison.Ordinal))
            {
                return null; // Attached property
            }

            if (SymbolHelper.DerivesFrom(elementType, parentType)
                || SymbolHelper.DerivesFrom(parentType, elementType))
            {
                return null;
            }

            return CreateIssue($"The property setter {syntax.Name.LocalName} is not valid when used inside {syntax.Parent.Name.LocalName}.", syntax, syntax.NameSpan).AsList();
        }
    }
}
