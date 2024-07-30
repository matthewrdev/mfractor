using System;
using System.Collections.Generic;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Code.WorkUnits;
using MFractor.Maui.Analysis.Fonts;
using MFractor.Maui.Fonts;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.CodeActions.Fix.Fonts
{
    class SetFontBuildActionToEmbeddedResource : FixCodeAction
    {
        public override string Documentation => "When an ExportedFont is used, however, the fonts build action is not set to EmbeddedResource, this code fix allows a developer to ";

        public override Type TargetCodeAnalyser => typeof(FontReferenceIsNotEmbeddedResource);

        public override string Identifier => "com.mfractor.code_fixes.xaml.set_font_build_action_to_embedded_resource";

        public override string Name => "Set Font Build Action To Embedded Resource";

        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlAttribute;

        protected override bool CanExecute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var embeddedFont = issue.GetAdditionalContent<IEmbeddedFont>();

            return embeddedFont != null;
        }

        protected override IReadOnlyList<ICodeActionSuggestion> Suggest(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, InteractionLocation location)
        {
            var embeddedFont = issue.GetAdditionalContent<IEmbeddedFont>();

            return CreateSuggestion("Set " + embeddedFont.LookupName + "'s build action to EmbeddedResource", 0).AsList();
        }

        protected override IReadOnlyList<IWorkUnit> Execute(ICodeIssue issue, XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context, ICodeActionSuggestion suggestion, InteractionLocation location)
        {
            var embeddedFont = issue.GetAdditionalContent<IEmbeddedFont>();

            var projectFile = ProjectService.GetProjectFileWithFilePath(embeddedFont.CompilationProject, embeddedFont.Font.FilePath);

            return new SetBuildActionWorkUnit()
            {
                BuildAction = "EmbeddedResource",
                ProjectFile = projectFile,
            }.AsList();
        }
    }
}

