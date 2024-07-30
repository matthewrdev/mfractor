using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.Commands.CompositeCommands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class InternalToolsCompositeCommand : CompositeCommand
    {
        [ImportingConstructor]
        public InternalToolsCompositeCommand([ImportMany]IEnumerable<IInternalToolCommand> commands)
            : base("Internal Tools", "Provides access to MFractors internal developer tools.", commands)
        {
        }

        public override ICommandState GetExecutionState(ICommandContext commandContext)
        {
#if !DEBUG
            return default;
#endif

#pragma warning disable CS0162 // Unreachable code detected
            return base.GetExecutionState(commandContext);
#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
