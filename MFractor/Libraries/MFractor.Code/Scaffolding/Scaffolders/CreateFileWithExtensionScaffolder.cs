using System;
using System.Collections.Generic;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;

namespace MFractor.Code.Scaffolding.Scaffolders
{
    class CreateFileWithExtensionScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Create File With Extension Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.create_file_with_extension";

        public override string Name => "Generate New File";

        public override string Documentation => "Creates a new file with the given file extension";

        public override string Criteria => "No special criteria required.";

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            if (string.IsNullOrEmpty(input.Name) || string.IsNullOrEmpty(input.Extension))
            {
                return false;
            }

            return true;
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion($"Create new {input.Extension} file", int.MinValue).AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            return new CreateProjectFileWorkUnit(string.Empty, input.RawInput, context.Project).AsList();
        }
    }
}