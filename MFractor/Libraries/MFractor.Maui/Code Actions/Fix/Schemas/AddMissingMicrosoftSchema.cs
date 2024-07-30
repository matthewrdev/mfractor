using System;
using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Schemas;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions
{
    class AddMissingMicrosoftSchema : FixCodeAction
    {
        public override string Documentation => "When the xmlns:x schema is missing from the root element, this code fix adds it.";

        public override Type TargetCodeAnalyser => typeof(MissingMicrosoftXamlSchema);

        public override string Identifier => "com.mfractor.code_fixes.xaml.add_missing_microsoft_schema";

        public override string Name => "Replace with correct attached property name";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlNode;

        protected override bool CanExecute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return true;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Add missing Microsoft schema").AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlNode syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            return new InsertXmlSyntaxWorkUnit()
            {
                FilePath = document.FilePath,
                HostSyntax = syntax,
                Syntax = new XmlAttribute("xmlns:x", XamlSchemas.MicrosoftSchemaUrl),
            }.AsList();
        }
    }
}

