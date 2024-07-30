using System;
using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class AboutCommandAdapter : IdeCommandAdapter<AboutCompositeCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
