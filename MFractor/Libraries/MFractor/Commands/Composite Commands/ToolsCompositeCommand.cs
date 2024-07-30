using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.Commands.CompositeCommands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ToolsCompositeCommand : CompositeCommand
    {
        [ImportingConstructor]
        public ToolsCompositeCommand([ImportMany]IEnumerable<IToolCommand> commands)
            : base("Tools", "Provides access to solution pad actions.", commands)
        {
        }
    }
}
