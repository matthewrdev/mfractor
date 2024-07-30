using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Triggers
{
    class TriggerIsMissingTargetType : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Node;

        public override string Identifier => "com.mfractor.code.analysis.xaml.trigger_is_missing_target_type";

        public override string Name => "Trigger Is Missing Target Type";

        public override string Documentation => "Inspects XAML elements that derive from `.TriggerBase` and validates that they include a `TargetType` attribute";

        public override string DiagnosticId => "MF1064";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlNode syntax,
                                                           IParsedXamlDocument document, 
                                                           IXamlFeatureContext context)
        {
            if (!context.Platform.SupportsTriggers)
            {
                return null;
            }

            var type = context.XamlSemanticModel.GetSymbol(syntax) as INamedTypeSymbol;


            var isTrigger = SymbolHelper.DerivesFrom(type, context.Platform.DataTrigger.MetaType) || SymbolHelper.DerivesFrom(type, context.Platform.MultiTrigger.MetaType);
            if (!isTrigger)
            {
                return null;
            }

            if (syntax.HasAttribute("TargetType"))
            {
                return null;
            }

            return CreateIssue("The trigger '" + syntax.Name.FullName + "' does not have a TargetType provided. This may cause a runtime crash.", syntax, syntax.NameSpan).AsList();
        }
    }
}
