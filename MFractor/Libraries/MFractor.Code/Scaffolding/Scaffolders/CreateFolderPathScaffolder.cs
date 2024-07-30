using System;
using System.Collections.Generic;
using MFractor.Work;

namespace MFractor.Code.Scaffolding.Scaffolders
{
    class CreateFolderPathScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "Create Folder Path Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.create_folder_path";

        public override string Name => "Generate Project Folders";

        public override string Documentation => "Creates a new file with the given file extension";

        public override string Criteria => "Activates when the scaffolding input ends with a path separator such as \\ or /.";

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return false;
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            throw new NotImplementedException();
        }
    }
}