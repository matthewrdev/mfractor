using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using MFractor.Commands.MainMenu.About;

namespace MFractor.Commands.MainMenu.Legal
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class LegalCompositeCommand : CompositeCommand
    {
        public LegalCompositeCommand()
            : base("Legal", "View MFractors privacy policy, terms of use and thirdy party software attribution")
        {
            this.With<PrivacyPolicyCommand>()
                .With<EndUserLicenseCommand>()
                .With<ThirdPartyAttributionCommand>();
        }
    }
}
