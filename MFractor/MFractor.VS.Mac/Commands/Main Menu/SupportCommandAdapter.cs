using System;
using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class SupportCommandAdapter : IdeCommandAdapter<SupportCompositeCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
