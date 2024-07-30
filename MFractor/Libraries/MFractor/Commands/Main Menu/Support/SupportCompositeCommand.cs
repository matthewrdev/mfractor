using System;
using System.ComponentModel.Composition;
using MFractor.Commands.MainMenu.Support;

namespace MFractor.Commands.MainMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class SupportCompositeCommand : CompositeCommand
    {
        public SupportCompositeCommand()
            : base("Support", "Get help and support for using MFractor")
        {
            this.With<SlackSupportCommand>()
                .With<GitterSupportCommand>()
                .With<TwitterSupportCommand>()
                .With<EmailSupportCommand>()
                .With<SubmitFeedbackCommand>()
                .With<DocumentationSupportCommand>();
        }
    }
}
