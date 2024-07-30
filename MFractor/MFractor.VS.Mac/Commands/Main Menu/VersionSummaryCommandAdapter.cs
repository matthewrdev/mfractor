using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class VersionSummaryCommandAdapter : IdeCommandAdapter<VersionSummaryCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
