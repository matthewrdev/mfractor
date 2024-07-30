using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class ViewLicenseCommandAdapter : IdeCommandAdapter<ViewLicenseCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
