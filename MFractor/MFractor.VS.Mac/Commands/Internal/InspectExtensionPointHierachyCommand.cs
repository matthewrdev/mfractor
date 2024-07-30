using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Commands;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Mac.Commands.Internal
{
    class InspectExtensionPointHierachyCommand : IInternalToolCommand
    {
        [ImportingConstructor]
        public InspectExtensionPointHierachyCommand(Lazy<IWorkEngine> workUnitEngine)
        {
            this.workUnitEngine = workUnitEngine;
        }

        readonly Lazy<IWorkEngine> workUnitEngine;
        public IWorkEngine  WorkUnitEngine => workUnitEngine.Value;

        public void Execute(ICommandContext commandContext)
        {
            static IReadOnlyList<IWorkUnit> textInputResultDelegate(string input)
            {
                Utilities.ExtensionPointHelper.RenderExtensionPointHierachy(input);

                return Array.Empty<IWorkUnit>();
            };

            var workUnit = new TextInputWorkUnit("Inspect Extension Point Hierachy",
                                                 "Enter the extension point hierachy to inspect",
                                                 Utilities.ExtensionPointHelper.MenuToolsPath,
                                                 "Confirm",
                                                 "Cancel",
                                                 textInputResultDelegate);

            WorkUnitEngine.ApplyAsync(workUnit);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState()
            {
                Label = "Inspect Extension Point Hierachy",
            };
        }
    }
}