using System;
using MFractor.Views.Settings.Commands;

namespace MFractor.VS.Mac.Commands.MainMenu.Preferences
{
    public class SettingsPreferencesCommandAdapter : IdeCommandAdapter<SettingsPreferencesCommand>
    {
        protected override bool RequiresActivation => false;
    }
}

