using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Utilities;
using MFractor.Work;

namespace MFractor.CSharp.Scaffolding
{
    class EventArgsDeclarationScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.csharp.event_args";

        public override string Name => "Generate EventArgs Declaration";

        public override string Documentation => "Inspects that the name provided into the composition engine starts ends with `EventArgs` to suggest creating a new EventArgs declaration.";

        public override string Criteria => "Activates when the scaffolding input is within a C# project and the file name ends with 'EventArgs'";

        [Import]
        public IEventArgsDeclarationGenerator EventArgsDeclarationGenerator { get; set; }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.NameNoExtension.EndsWith("EventArgs", StringComparison.OrdinalIgnoreCase)
                   && context.Project != null;
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return CreateSuggestion("Create a new EventArgs named " + input.Name).AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            return EventArgsDeclarationGenerator.Generate(input.NameNoExtension, input.FolderPath, context.Project);
        }
    }
}