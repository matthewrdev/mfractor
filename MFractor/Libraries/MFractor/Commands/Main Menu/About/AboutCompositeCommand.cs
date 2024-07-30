using System.ComponentModel.Composition;
using MFractor.Commands.MainMenu.About;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class AboutCompositeCommand : CompositeCommand
    {
        public AboutCompositeCommand()
            : base("About", "Edit your activation and licensing, see MFractors about information and more.")
        {
            this.With<AboutProductCommand>()
                .With<ActivationCommand>()
                .With<ViewReleaseNotesCommand>()
                .With<OnboardingCommand>();
        }
    }
}
