using System;
using System.ComponentModel.Composition;

namespace MFractor.Views.Settings.Commands
{
    [Export]
    public class SettingsPreferencesCommand : PreferencesCommand<SettingsWidget>
    {
        protected override string Title => "General";
    }
}

