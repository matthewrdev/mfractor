using System;
using MFractor.Maui.Mvvm;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;

namespace MFractor.Maui.WorkUnits
{
    public class ConfigureProjectMvvmSettingsWorkUnit : WorkUnit
    {
        public IXamlPlatform Platform { get; set; }

        public ProjectIdentifier ProjectIdentifier { get; set; }

        public Action<ProjectIdentifier, ProjectMvvmSettings> OnCompleted { get; set; }
    }
}