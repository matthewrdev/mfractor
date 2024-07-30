using System;
using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class ViewReleaseNotesCommandAdapter : IdeCommandAdapter<ViewReleaseNotesCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
