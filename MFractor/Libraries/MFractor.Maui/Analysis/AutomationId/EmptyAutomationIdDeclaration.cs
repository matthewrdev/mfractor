using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.AutomationId
{
    class EmptyAutomationIdDeclaration : XamlCodeAnalyser
    {
        public override string Documentation => "Inspects a Xaml document for occurances of duplicate `AutomationId` declarations.";

        public override IssueClassification Classification => IssueClassification.Warning;

        public override string Identifier => "com.mfractor.code.analysis.xaml.empty_automation_id";

        public override string Name => "Empty AutomationId Declaration";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1001";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "AutomationId"
                || syntax.HasValue)
            {
                return null;
            }

            return CreateIssue($"This AutomationId has no value assigned. Is this intended?", syntax, syntax.Span).AsList();
        }
    }
}

