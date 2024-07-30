using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Ide.DeleteOutputFolders.Models;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace MFractor.Ide.DeleteOutputFolders
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IDeleteOutputFoldersConfigurationService))]
    class DeleteOutputFoldersConfigurationService : IDeleteOutputFoldersConfigurationService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public DeleteOutputFoldersConfigurationService(Lazy<IApplicationPaths> applicationPaths,
                                                       Lazy<IProjectService> projectService)
        {
            this.applicationPaths = applicationPaths;
            this.projectService = projectService;
        }

        const string preferencesFileName = "delete-output-folders.rules";

        string PreferencesFilePath => Path.Combine(ApplicationPaths.ApplicationDataPath, preferencesFileName);

        Dictionary<string, DeleteOutputFoldersConfiguration> configurations = new Dictionary<string, Models.DeleteOutputFoldersConfiguration>();

        public event EventHandler ConfigurationsChanged;

        void PersistConfigurations()
        {
            if (File.Exists(PreferencesFilePath))
            {
                File.Delete(PreferencesFilePath);
            }

            try
            {
                var content = JsonConvert.SerializeObject(configurations, Newtonsoft.Json.Formatting.Indented);

                File.WriteAllText(PreferencesFilePath, content);
            }
            catch (Exception ex) // Shouldn't happen, indicates access permissions.
            {
                log?.Exception(ex);
            }
        }

        void ReloadConfigurations()
        {
            if (File.Exists(PreferencesFilePath))
            {
                try
                {
                    var content = File.ReadAllText(PreferencesFilePath);

                    configurations = JsonConvert.DeserializeObject<Dictionary<string, DeleteOutputFoldersConfiguration>>(content) ?? new Dictionary<string, DeleteOutputFoldersConfiguration>();
                }
                catch (Exception ex) // Shouldn't happen, indicates data-corruption.
                {
                    log?.Exception(ex);
                    Clear();
                }
            }
            else
            {
                configurations = new Dictionary<string, DeleteOutputFoldersConfiguration>();
            }

            if (configurations == null)
            {
                configurations = new Dictionary<string, DeleteOutputFoldersConfiguration>();
            }
        }


        public bool HasConfiguration(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("message", nameof(identifier));
            }

            return configurations.ContainsKey(identifier);
        }

        public bool HasConfiguration(Solution solution)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var identifier = GetIdentifier(solution);

            return HasConfiguration(identifier);
        }

        public bool HasConfiguration(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var identifier = GetIdentifier(project);

            return HasConfiguration(identifier);
        }

        public IReadOnlyList<IDeleteOutputFoldersConfiguration> Configurations => configurations.Values.Cast<IDeleteOutputFoldersConfiguration>().ToList();

        public void Clear()
        {
            if (File.Exists(PreferencesFilePath))
            {
                File.Delete(PreferencesFilePath);
            }

            configurations = new Dictionary<string, DeleteOutputFoldersConfiguration>();
        }

        public string GetIdentifier(Solution solution)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            return Path.GetFileNameWithoutExtension(Path.GetFileName(solution.FilePath));
        }

        public string GetIdentifier(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var solutionId = GetIdentifier(project.Solution);

            return solutionId + "." + ProjectService.GetProjectGuid(project);
        }

        public IDeleteOutputFoldersOptions GetOptionsOrDefault(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("message", nameof(identifier));
            }

            if (configurations.TryGetValue(identifier, out var config))
            {
                return config.Options;
            }

            return new DeleteOutputFoldersOptions();
        }

        public IDeleteOutputFoldersOptions GetOptionsOrDefault(Solution solution)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            var identifier = GetIdentifier(solution);

            return GetOptionsOrDefault(identifier);
        }

        public IDeleteOutputFoldersOptions GetOptionsOrDefault(Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var identifier = GetIdentifier(project);

            return GetOptionsOrDefault(identifier);
        }

        public void SetOptions(IDeleteOutputFoldersConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            SetOptions(configuration.Name, configuration.Identifier, configuration.Options);
        }

        public void SetOptions(string name, string identifier, IDeleteOutputFoldersOptions options)
        {
            if (configurations.ContainsKey(identifier))
            {
                configurations.Remove(identifier);
            }

            configurations[identifier] = new DeleteOutputFoldersConfiguration(name, identifier, options);

            NotifyConfigurationChanged();

            PersistConfigurations();
        }

        void NotifyConfigurationChanged()
        {
            try
            {
                ConfigurationsChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void SetOptions(Solution solution, IDeleteOutputFoldersOptions options)
        {
            if (solution is null)
            {
                throw new ArgumentNullException(nameof(solution));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var identifer = GetIdentifier(solution);
            var name = Path.GetFileName(solution.FilePath);

            SetOptions(name, identifer, options);
        }

        public void SetOptions(Project project, IDeleteOutputFoldersOptions options)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var identifer = GetIdentifier(project);
            var name = Path.GetFileName(project.FilePath);

            var solutionIdentifier = GetIdentifier(project.Solution);

            SetOptions(name, identifer, options);
        }

        public void Startup()
        {
            ReloadConfigurations();
        }

        public void Shutdown()
        {
        }
    }
}
