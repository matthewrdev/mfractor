using System;
using MFractor.Views.Settings.Commands;

namespace MFractor.VS.Mac.Commands.MainMenu.Preferences
{
    public class CodeAnalysisPreferencesCommandAdapter : IdeCommandAdapter<CodeAnalysisPreferencesCommand>
    {
        protected override bool RequiresActivation => false;
    }
}

