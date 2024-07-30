using System;
using System.ComponentModel.Composition;

namespace MFractor.Views.Settings.Commands
{
    [Export]
    public class CodeAnalysisPreferencesCommand : PreferencesCommand<CodeAnalysisWidget>
    {
        protected override string Title => "Code Analysis";
    }
}

