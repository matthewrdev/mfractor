using System;
using System.ComponentModel.Composition;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class AboutProductCommand : WorkUnitCommand
    {
        [ImportingConstructor]
        public AboutProductCommand(Lazy<IWorkEngine> workEngine, IProductInformation productInformation)
            : base($"About {productInformation.ProductName}",
                   $"View the build and copyright details for {productInformation.ProductName}",
                   new AboutDialogWorkUnit(),
                   workEngine)
        {
        }
    }
}
