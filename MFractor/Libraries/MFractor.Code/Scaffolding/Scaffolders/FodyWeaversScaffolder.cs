using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.Fody;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.Code.Scaffolding.Scaffolders
{
    class FodyWeaversScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => "FodyWeavers Scaffolder";

        public override string Identifier => "com.mfractor.Code.Scaffolding.xaml.fody_weavers";

        public override string Name => "Generate FodyWeavers.xml";

        public override string Documentation => "Generates new FodyWeavers.xml file.";

        public override string Criteria => "Activates when the scaffolding input matches 'FodyWeavers.xml'";

        [Import]
        public IFodyWeaversGenerator FodyWeaversGenerator
        {
            get;
            set;
        }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.Name.Equals("FodyWeavers.xml", StringComparison.OrdinalIgnoreCase);
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            return FodyWeaversGenerator.Generate(context.Project, input.RawInput);
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create FodyWeavers Configuration", int.MaxValue).AsList();
        }
    }
}
