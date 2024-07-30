using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Maui.CodeActions.Generate;
using MFractor.Maui.CodeGeneration.Mvvm;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using Newtonsoft.Json;

namespace MFractor.Maui.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IProjectMvvmSettingsService))]
    class ProjectMvvmSettingsService : IProjectMvvmSettingsService
    {
        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine;
        IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        [ImportingConstructor]
        public ProjectMvvmSettingsService(Lazy<IUserOptions> userOptions,
                                          Lazy<IConfigurationEngine> configurationEngine)
        {
            this.userOptions = userOptions;
            this.configurationEngine = configurationEngine;
        }

        public ProjectMvvmSettings CreateDefault(ProjectIdentifier projectIdentifier, IXamlPlatform platform)
        {
            var configurationId = ConfigurationId.Create(projectIdentifier);

            var viewViewModelGenerator = ConfigurationEngine.Resolve<IViewViewModelGenerator>(configurationId);
            var generateNewViewModel = ConfigurationEngine.Resolve<GenerateNewViewModel>(configurationId);

            var options = new ProjectMvvmSettings
            {
                ViewSuffix = viewViewModelGenerator.ViewSuffix,
                ViewModelSuffix = generateNewViewModel.ViewModelSuffix,
                ViewBaseClassXmlnsPrefix = viewViewModelGenerator.ViewXmlnsPrefix,
                ViewFolder = viewViewModelGenerator.ViewsFolder,
                ViewBaseClass = platform.Page.MetaType,
                ViewModelFolder = generateNewViewModel.ViewModelsFolder,
                ViewModelBaseClass = generateNewViewModel.BaseClass,
                ViewModelProjectId = projectIdentifier.Guid,
                ViewProjectId = projectIdentifier.Guid,
                BindingContextConnectorId = viewViewModelGenerator.DefaultBindingContextConnectorId,
            };

            return options;
        }

        public string GetPersistenceKey(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier == null)
            {
                throw new ArgumentNullException(nameof(projectIdentifier));
            }

            const string baseKey = "com.mfractor.settings.mvvm_generation.";

            return baseKey + projectIdentifier.Guid;
        }

        public ProjectMvvmSettings Load(ProjectIdentifier projectIdentifier, IXamlPlatform platform, bool createDefaultIfEmpty = true)
        {
            var key = GetPersistenceKey(projectIdentifier);

            ProjectMvvmSettings options = null;

            if (UserOptions.HasKey(key))
            {
                var content = UserOptions.Get(key, default(string));

                if (!string.IsNullOrEmpty(content))
                {
                    content = Base64Helper.Decode(content);

                    try
                    {
                        options = JsonConvert.DeserializeObject<ProjectMvvmSettings>(content);
                    }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                    catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }
            }

            if (options == null && createDefaultIfEmpty)
            {
                options = CreateDefault(projectIdentifier, platform);
            }

            return options;
        }

        public void Save(ProjectIdentifier projectIdentifier, ProjectMvvmSettings settings)
        {
            var key = GetPersistenceKey(projectIdentifier);

            if (settings != null)
            {
                var content = JsonConvert.SerializeObject(settings);

                content = Base64Helper.Encode(content);

                UserOptions.Set(key, content);
            }
            else
            {
                UserOptions.Set(key, string.Empty);
            }
        }
    }
}