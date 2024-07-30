using System;
using System.ComponentModel.Composition;

namespace MFractor.Views.Settings.Commands
{
    [Export]
    public class FormattingOptionsPreferencesCommand : PreferencesCommand<FormattingOptionsWidget>
    {
        protected override string Title => "Formatting Options";
    }
}

