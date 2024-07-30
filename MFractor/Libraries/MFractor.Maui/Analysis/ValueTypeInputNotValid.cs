using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class ValueTypeInputNotValid : XamlCodeAnalyser
	{
        public override string Documentation => "Inspects the input provided to attributes that expect value types (int, bool, double etc) and validates that the input is valid.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.value_type_input_not_valid";

        public override string Name => "Validate Value Types";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1074";

        protected override IReadOnlyList<ICodeIssue> Analyse (XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			if (!syntax.HasValue)
            {
                return null;
            }

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return null;
            }

			var symbol = context.XamlSemanticModel.GetSymbol(syntax);
            if (symbol == null)
            {
                return null;
            }

            var isPropertyOrField = (symbol is IPropertySymbol || symbol is IFieldSymbol);
            if (!isPropertyOrField)
            {
                return null;
            }

			if (FormsSymbolHelper.HasTypeConverterAttribute(symbol, context.Platform))
            {
                return null;
            }

			var memberType = symbol is IPropertySymbol ? (symbol as IPropertySymbol).Type : (symbol as IFieldSymbol).Type;

			var specialType = memberType.SpecialType;

			if (!SymbolHelper.IsPrimitiveValueType(specialType))
            {
                return null;
            }

            if (SymbolHelper.CanParseAsPrimitiveValueType(syntax.Value.Value, specialType))
			{
                return null;
			}

            return CreateIssue($"{syntax.Value} is not a valid {memberType} value.", syntax, syntax.Value.Span).AsList();
        }
	}
}

