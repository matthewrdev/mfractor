using System;
using System.Collections.Generic;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Code.Scaffolding;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;

namespace MFractor.Linker.CodeCompletion
{
    class LinkerConfigurationScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Linker Configuration";

        public override string Identifier => "com.mfractor.Code.Scaffolding.linker.configuration";

        public override string Name => "Linker Configuration File";

        public override string Documentation => "Generate a linker.xml configuration file for Android and iOS projects.";

        public override string Criteria => "Activates when the project is an iOS or Android project and the file name is 'linker.xml'.";

        [ExportProperty("The code snippet to use when creating a Linker.xml")]
        [CodeSnippetResource("Resources/Snippets/linker.txt")]
        public ICodeSnippet Snippet { get; set; }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return context.Project.IsMobileProject() && input.Name.Equals("linker.xml", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create Linker.xml configuration", int.MaxValue).AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            return new CreateProjectFileWorkUnit()
            {
                FileContent = Snippet.ToString(),
                FilePath = input.RawInput,
                TargetProject = context.Project,
                BuildAction = "LinkDescription"
            }.AsList();
        }
    }
}
