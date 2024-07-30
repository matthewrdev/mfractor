using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    class BindingExpressionReturnTypeDoesNotMatchExpectedTypeAnalysis : ExpressionTypeAnalysisRoutine<BindingExpression>
    {
        public override string Documentation => "Validates that the .NET symbol returned by a binding expression matches the expected type for the property.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.binding_expression_does_not_match_expected_type";

        public override string Name => "Binding Expression Return Type Mismatch";

        public override string DiagnosticId => "MF1010";

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(BindingExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var bindingContext = context.XamlSemanticModel.GetBindingContext(expression, context);

            var outerType = context.XamlSemanticModel.GetSymbol(expression.ParentAttribute) as IPropertySymbol;
            if (outerType != null && SymbolHelper.DerivesFrom(outerType.Type, context.Platform.BindingBase.MetaType))
            {
                return null;
            }

            if (bindingContext == null)
            {
                return null;
            }

            if (!expression.HasReferencedSymbol)
            {
                return null; // Can't evalaute.
            }

            if (expression.Converter != null)
            {
                return null; // Can't evaulate when a converter is in play.
            }

            var attr = expression.ParentAttribute;
            var symbol = context.XamlSemanticModel.GetDataBindingExpressionResult(expression, context) as ISymbol;
            var parentSymbol = context.XamlSemanticModel.GetSymbol(attr) as ISymbol;

            if (symbol == null || parentSymbol == null)
            {
                return null;
            }

            if (FormsSymbolHelper.HasTypeConverterAttribute(parentSymbol, context.Platform))
            {
                return null; // Value converter in play, not relevant.
            }

            var returnedType = (expression.ReferencesBindingContext ? bindingContext : SymbolHelper.ResolveMemberReturnType(symbol)) as INamedTypeSymbol;
            var expectedType = SymbolHelper.ResolveMemberReturnType(parentSymbol) as INamedTypeSymbol;

            if (returnedType == null
                || expectedType == null)
            {
                return null;
            }

            var parent = expression.ParentAttribute.Parent;
            if (!FormsSymbolHelper.IsTypeMismatch(expectedType, returnedType, parent, context.Namespaces, context.XmlnsDefinitions, context.Project, context.XamlSemanticModel, context.Platform))
            {
                return null;
            }

            if (IsExemptBindingReturnTypeFlow(returnedType, expression.ParentAttribute, context.Compilation, context.XamlSemanticModel, context.Platform))
            {
                return null;
            }

            return CreateIssue($"This binding expression returns a '{returnedType.ToString()}' but '{attr.Name.LocalName}' expects a '{expectedType.ToString()}' typed value.", expression.ParentAttribute, expression.Span).AsList();
        }

        bool IsExemptBindingReturnTypeFlow(INamedTypeSymbol bindingReturnType,
                                           XmlAttribute parentAttribute,
                                           Compilation compilation,
                                           IXamlSemanticModel xamlSemanticModel,
                                           IXamlPlatform platform)
        {
            if (parentAttribute.Name.LocalName != platform.ItemSourceProperty)
            {
                return false;
            }

            var parentType = xamlSemanticModel.GetSymbol(parentAttribute) as INamedTypeSymbol;
            if (parentType == null)
            {
                return false;
            }
            var pickerType = compilation.GetTypeByMetadataName(platform.Picker.MetaType);
            var listView = compilation.GetTypeByMetadataName(platform.ListView.MetaType);

            if (pickerType == null || listView == null)
            {
                return false;
            }

            if (SymbolHelper.DerivesFrom(parentType, listView)
                || SymbolHelper.DerivesFrom(parentType, pickerType))
            {
                return SymbolHelper.DerivesFrom(bindingReturnType, "System.Collections.IEnumerable");
            }

            return false;
        }
    }
}
