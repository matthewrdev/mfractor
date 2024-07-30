using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class LicenseSummaryCommandAdapter : IdeCommandAdapter<LicenseSummaryCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
