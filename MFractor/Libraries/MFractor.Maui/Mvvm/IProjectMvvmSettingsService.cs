using System;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Mvvm
{
    public interface IProjectMvvmSettingsService
    {
        string GetPersistenceKey(ProjectIdentifier projectIdentifier);

        ProjectMvvmSettings Load(ProjectIdentifier projectIdentifier, IXamlPlatform platform, bool createDefaultIfEmpty = true);

        ProjectMvvmSettings CreateDefault(ProjectIdentifier projectIdentifier, IXamlPlatform platform);

        void Save(ProjectIdentifier projectIdentifier, ProjectMvvmSettings settings);
    }
}
