using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class StyleIsMissingTargetTypeAnalysis : XamlCodeAnalyser
    {
        public override string Documentation => "When a `Style` is used within XAML, it should always specify a type it targets using the `TargetType` property. This analysis check inspects for usages of `Style` that don't assign the `TargetType` property.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.style_is_missing_target_type";

        public override string Name => "Style Is Missing TargetType";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string DiagnosticId => "MF1059";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as ITypeSymbol;
            if (!SymbolHelper.DerivesFrom(symbol, context.Platform.Style.MetaType))
            {
                return null;
            }

            if (syntax.HasAttribute((attr) => attr.Name.LocalName == "TargetType"))
            {
                return null;
            }

            // Find the nearest and pass to fix.
            return CreateIssue("This style is missing a 'TargetType'", syntax, syntax.NameSpan).AsList();
        }
    }
}

