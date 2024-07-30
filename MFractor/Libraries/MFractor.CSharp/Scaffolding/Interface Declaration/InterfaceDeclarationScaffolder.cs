using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Scaffolding;
using MFractor.CSharp.CodeGeneration;
using MFractor.Work;
using MFractor.Utilities;

namespace MFractor.CSharp.Scaffolding
{
    class InterfaceDeclarationScaffolder : Scaffolder
    {
        public override string AnalyticsEvent => Name;

        public override string Identifier => "com.mfractor.Code.Scaffolding.csharp.interface";

        public override string Name => "Generate Interface Declaration";

        public override string Documentation => "Inspects that the name provided into the composition engine starts with `I` and ends with `.cs` to suggest creating a new interface.";

        public override string Criteria => "Activates when the scaffolding input is within a C# project and the file name starts with 'I''";

        [Import]
        public IInterfaceDeclarationGenerator InterfaceDeclarationGenerator { get; set; }

        public override bool CanProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            return input.Name.StartsWith("I", StringComparison.Ordinal)
                    && input.Name.Length >= 2
                    && char.IsUpper(input.Name[1])
                   && context.Project != null;
        }

        public override IReadOnlyList<IScaffoldingSuggestion> SuggestScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state)
        {
            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            return CreateSuggestion("Create a new interface named " + className).AsList();
        }

        public override IReadOnlyList<IWorkUnit> ProvideScaffolds(IScaffoldingContext context, IScaffoldingInput input, IScaffolderState state , IScaffoldingSuggestion suggestion)
        {
            var className = CSharpNameHelper.ConvertToValidCSharpName(input.NameNoExtension);

            return InterfaceDeclarationGenerator.Generate(className, input.FolderPath, context.Project);
        }
    }
}
