using System;
using MFractor.Views.Settings.Commands;

namespace MFractor.VS.Mac.Commands.MainMenu.Preferences
{
    public class DeleteOutputOptionsPreferencesCommandAdapter : IdeCommandAdapter<DeleteOutputOptionsPreferencesCommand>
    {
        protected override bool RequiresActivation => false;
    }
}

