using System;
using MFractor.Maui.Mvvm;

namespace MFractor.Views.MVVMWizard.Settings
{
    public class ProjectMvvmSettingsSavedEventArgs : EventArgs
    {
        public ProjectMvvmSettings Settings { get; }

        public ProjectIdentifier ProjectIdentifier { get; }

        public ProjectMvvmSettingsSavedEventArgs(ProjectMvvmSettings options,
                                                 ProjectIdentifier projectIdentifier)
        {
            Settings = options;
            ProjectIdentifier = projectIdentifier;
        }
    }
}
