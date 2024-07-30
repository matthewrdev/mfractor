using System;
using System.ComponentModel.Composition;

namespace MFractor.Views.Settings.Commands
{
    [Export]
    public class DeleteOutputOptionsPreferencesCommand : PreferencesCommand<DeleteOutputFoldersOptionsWidget>
    {
        protected override string Title => "Delete Output Folders";
    }
}

