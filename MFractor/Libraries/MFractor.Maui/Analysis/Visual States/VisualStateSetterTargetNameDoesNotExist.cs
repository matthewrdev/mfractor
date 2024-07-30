using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.VisualStates
{
    class VisualStateSetterTargetNameDoesNotExist : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Warning;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.visual_state_setter_target_name_does_not_exist";

        public override string Name => "VisualState Setter TargetName Does Not Exist";

        public override string Documentation => "Inspects Setters within VisualStates and verifies that the TargetName exists in the current document";

        public override string DiagnosticId => "MF1101";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.FullName != "TargetName" || !syntax.HasValue) 
            {
                return null;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(type, context.Platform.Setter.MetaType))
            {
                return null;
            }

            var visualStateParent = context.XamlSemanticModel.GetSymbol(syntax.Parent.Parent?.Parent) as INamedTypeSymbol;
            if (!SymbolHelper.DerivesFrom(visualStateParent, context.Platform.VisualState.MetaType))
            {
                return null;
            }

            var codeBehindFields = context.XamlSemanticModel.CodeBehindFields;

            var codeBehindField = codeBehindFields.GetCodeBehindField(syntax.Value.Value);

            if (codeBehindField != null)
            {
                return null;
            }

            var value = syntax.Value.Value;

            var nearestMatch = SuggestionHelper.FindBestSuggestion(value, codeBehindFields.CodeBehindFields.Keys);

            var message = $"No element named '{value}' is declared in this document using an x:Name expression.";

            var bundle = new VisualStateSetterTargetNameDoesNotExistBundle(nearestMatch);

            if (!string.IsNullOrEmpty(nearestMatch))
            {
                message += $"\n\nDid you mean {nearestMatch}?";
            }

            return CreateIssue(message, syntax, syntax.Value.Span, bundle).AsList();
        }
    }
}