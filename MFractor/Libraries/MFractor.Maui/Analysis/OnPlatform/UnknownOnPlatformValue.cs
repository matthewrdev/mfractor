using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.OnPlatform
{
    class UnknownOnPlatformValue : XamlCodeAnalyser
	{
		public override string Documentation => "Checks the Platform value provided to a `.On` element and verifies that it is a known platform within the platforms Device class.";

		public override IssueClassification Classification => IssueClassification.Warning;

		public override string Identifier => "com.mfractor.code.analysis.xaml.unknown_platform";

		public override string Name => "Unknown OnPlatform Value";

		public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

		public override string DiagnosticId => "MF1097";

		protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
		{
			var symbol = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;
			var parentSymbol = context.XamlSemanticModel.GetSymbol(syntax?.Parent) as INamedTypeSymbol;
			if (symbol == null
				|| parentSymbol is null
				|| !context.Platform.SupportsOnPlatform
				|| !context.Platform.SupportsDevice)
			{
				return null;
			}

            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.On.MetaType)
                || !SymbolHelper.DerivesFrom(parentSymbol, context.Platform.OnPlatform.MetaType))
            {
				return null;
            }

			var platformAttribute = syntax.GetAttributeByName("Platform");
            if (platformAttribute == null
                || !platformAttribute.HasValue
                || ExpressionParserHelper.IsExpression(platformAttribute.Value.Value))
			{
				return null;
            }

			var value = platformAttribute.Value.Value;

			var device = context.Compilation.GetTypeByMetadataName(context.Platform.Device.MetaType);

			var fields = SymbolHelper.GetAllMemberSymbols<IFieldSymbol>(device)
							          .Where(f => f.Type.SpecialType == SpecialType.System_String && f.IsConst);

            if (fields.Any(f => f.ConstantValue.ToString() == value))
            {
				return null;
            }

			var fieldValues = fields.Select(f => f.ConstantValue.ToString()).ToList();

			var message = $"{value} is not an available platform.";
			var nearest = SuggestionHelper.FindBestSuggestion(value, fieldValues);
			if (!string.IsNullOrEmpty(nearest))
			{
				message += $"\n\nDid you mean '{nearest}'?";
			}

			return CreateIssue(message, platformAttribute, platformAttribute.Value.Span, nearest).AsList();
		}
	}
}
