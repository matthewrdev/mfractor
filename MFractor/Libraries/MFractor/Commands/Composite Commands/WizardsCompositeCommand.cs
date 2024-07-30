using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MFractor.Commands.CompositeCommands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class WizardsCompositeCommand : CompositeCommand
    {
        [ImportingConstructor]
        public WizardsCompositeCommand([ImportMany]IEnumerable<IWizardCommand> commands)
            : base("Wizards", "Open one of MFractors code generation wizards.", commands)
        {
        }
    }
}
