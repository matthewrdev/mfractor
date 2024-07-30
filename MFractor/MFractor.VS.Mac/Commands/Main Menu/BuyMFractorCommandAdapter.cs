using System;
using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class BuyMFractorCommandAdapter : IdeCommandAdapter<PurchaseCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
