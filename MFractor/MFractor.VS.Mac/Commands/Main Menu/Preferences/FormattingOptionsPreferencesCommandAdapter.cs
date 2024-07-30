using System;
using MFractor.Views.Settings.Commands;

namespace MFractor.VS.Mac.Commands.MainMenu.Preferences
{
    public class FormattingOptionsPreferencesCommandAdapter : IdeCommandAdapter<FormattingOptionsPreferencesCommand>
    {
        protected override bool RequiresActivation => false;
    }
}

