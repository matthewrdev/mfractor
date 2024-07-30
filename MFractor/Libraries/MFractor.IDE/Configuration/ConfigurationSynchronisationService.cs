using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Ide.Configuration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ConfigurationSynchronisationService))]
    sealed class ConfigurationSynchronisationService : IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IConfigurationRepository> configurationRepository;
        IConfigurationRepository ConfigurationRepository => configurationRepository.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public ConfigurationSynchronisationService(Lazy<IConfigurationRepository> configurationRepository,
                                                   Lazy<IWorkspaceService> workspaceService,
                                                   Lazy<IProjectService> projectService)
        {
            this.configurationRepository = configurationRepository;
            this.workspaceService = workspaceService;
            this.projectService = projectService;
        }

        public void Startup()
        {
            WorkspaceService.SolutionOpened += WorkspaceService_SolutionOpened;
            WorkspaceService.SolutionClosed += WorkspaceService_SolutionClosed;
            WorkspaceService.ProjectAdded += WorkspaceService_ProjectAdded;
            WorkspaceService.ProjectRenamed += WorkspaceService_ProjectRenamed;
            WorkspaceService.ProjectRemoved += WorkspaceService_ProjectRemoved;
            WorkspaceService.ProjectReferenceAdded += WorkspaceService_ProjectReferenceAdded;
            WorkspaceService.ProjectReferenceRemoved += WorkspaceService_ProjectReferenceRemoved;
            WorkspaceService.FilesChanged += WorkspaceService_FilesChanged;
            WorkspaceService.FilesRenamed += WorkspaceService_FileRenamed;
            WorkspaceService.FilesAddedToProject += WorkspaceService_FileAddedToProject;
            WorkspaceService.FilesRemovedFromProject += WorkspaceService_FileRemovedFromProject;
        }

        public void Shutdown()
        {
            WorkspaceService.SolutionOpened -= WorkspaceService_SolutionOpened;
            WorkspaceService.SolutionClosed -= WorkspaceService_SolutionClosed; ;
            WorkspaceService.ProjectAdded -= WorkspaceService_ProjectAdded;
            WorkspaceService.ProjectRenamed -= WorkspaceService_ProjectRenamed;
            WorkspaceService.ProjectRemoved -= WorkspaceService_ProjectRemoved;
            WorkspaceService.ProjectReferenceAdded -= WorkspaceService_ProjectReferenceAdded;
            WorkspaceService.ProjectReferenceRemoved -= WorkspaceService_ProjectReferenceRemoved;
            WorkspaceService.FilesChanged -= WorkspaceService_FilesChanged;
            WorkspaceService.FilesRenamed -= WorkspaceService_FileRenamed;
            WorkspaceService.FilesAddedToProject -= WorkspaceService_FileAddedToProject;
            WorkspaceService.FilesRemovedFromProject -= WorkspaceService_FileRemovedFromProject;
        }

        void WorkspaceService_SolutionOpened(object sender, SolutionOpenedEventArgs e)
        {
            var solution = WorkspaceService.GetSolution(e.SolutionName);

            if (solution != null)
            {
                foreach (var project in solution.Projects)
                {
                    AddProjectConfiguration(project);
                }
            }
        }

        void WorkspaceService_SolutionClosed(object sender, SolutionClosedEventArgs e)
        {
            var solution = WorkspaceService.GetSolution(e.SolutionName);

            if (solution != null)
            {
                foreach (var p in solution.Projects)
                {
                    var guid = ProjectService.GetProjectGuid(p);
                    ConfigurationRepository.RemoveConfiguration(guid);
                }
            }
        }

        void WorkspaceService_ProjectAdded(object sender, ProjectAddedEventArgs e)
        {
            var project = ProjectService.GetProject(e.ProjectGuid);
            if (project != null)
            {
                AddProjectConfiguration(project);
            }
        }

        void WorkspaceService_ProjectRenamed(object sender, ProjectRenamedEventArgs e)
        {
        }

        void WorkspaceService_ProjectRemoved(object sender, ProjectRemovedEventArgs e)
        {
            ConfigurationRepository.RemoveConfiguration(e.ProjectGuid);
        }

        void WorkspaceService_ProjectReferenceAdded(object sender, ProjectReferenceAddedEventArgs e)
        {
        }

        void WorkspaceService_ProjectReferenceRemoved(object sender, ProjectReferenceRemovedEventArgs e)
        {
            var project = ProjectService.GetProject(e.ProjectGuid);

            if (project != null)
            {
                var id = ConfigurationId.Create(e.ProjectGuid, project.Name);
                ConfigurationRepository.RemoveReferenceConfiguration(id, e.ReferenceName);
            }
        }

        void WorkspaceService_FilesChanged(object sender, FilesEventArgs e)
        {
            foreach (var guid in e.ProjectGuids)
            {
                var project = ProjectService.GetProject(guid);

                if (project is null)
                {
                    continue;
                }

                var id = ConfigurationId.Create(project.GetIdentifier());

                foreach (var file in e.GetProjectFiles(guid))
                {
                    if (IsConfigurationFile(file))
                    {
                        SaveConfiguration(id, file);
                    }
                }
            }
        }

        void WorkspaceService_FileRenamed(object sender, FilesRenamedEventArgs e)
        {
            foreach (var guid in e.ChangeSet.ProjectGuids)
            {
                var project = ProjectService.GetProject(guid);
                var id = ConfigurationId.Create(project.GetIdentifier());

                foreach (var file in e.GetProjectFiles(guid))
                {
                    if (IsConfigurationFile(file.OldFilePath))
                    {
                        ConfigurationRepository.RemoveConfiguration(guid, project.Name, file.OldFilePath);
                    }

                    if (IsConfigurationFile(file.NewFilePath))
                    {
                        SaveConfiguration(id, file.NewFilePath);

                    }
                }
            }
        }

        void WorkspaceService_FileAddedToProject(object sender, FilesEventArgs e)
        {
            foreach (var guid in e.ProjectGuids)
            {
                var configFile = e.GetProjectFiles(guid)?.FirstOrDefault(file => IsConfigurationFile(file));

                if (!string.IsNullOrEmpty(configFile))
                {
                    var id = ConfigurationId.Create(guid, configFile);
                    SaveConfiguration(id, configFile);
                }
            }
        }

        void WorkspaceService_FileRemovedFromProject(object sender, FilesEventArgs e)
        {
            foreach (var guid in e.ProjectGuids)
            {
                var configFile = e.GetProjectFiles(guid)?.FirstOrDefault(file => IsConfigurationFile(file));

                if (!string.IsNullOrEmpty(configFile))
                {
                    var id = ConfigurationId.Create(guid, configFile);
                    ConfigurationRepository.RemoveConfiguration(id, configFile);
                }
            }
        }

        void AddProjectConfiguration(Project project)
        {
            if (project == null || !File.Exists(project.FilePath))
            {
                log?.Warning("The project " + project.Name + " does not exist on disk");
                return;
            }

            var guid = ProjectService.GetProjectGuid(project);

            var id = ConfigurationId.Create(guid, project.Name);

            var files = ProjectService.GetProjectFiles(project);

            var configs = files.Where(pf => IsConfigurationFile(pf.FilePath)).Select(pf => pf.FilePath);

            foreach (var c in configs)
            {
                SaveConfiguration(id, c);
            }

            TryProcessSilentConfig(project, id, configs);
        }

        void SaveConfiguration(ConfigurationId id, string filePath)
        {
            var result = ConfigurationRepository.SaveConfiguration(id, filePath, out var failureMessage);
        }

        void TryProcessSilentConfig(Project project,
                                    ConfigurationId id,
                                    IEnumerable<string> configs)
        {
            var projectFolder = new FileInfo(project.FilePath).Directory.FullName;

            var silentConfig = Path.Combine(projectFolder, FileExtensions.ConfigurationFileExtension);
            if (File.Exists(silentConfig)
                && !configs.Any(cf => Path.GetFileName(cf) == FileExtensions.ConfigurationFileExtension))
            {
                SaveConfiguration(id, silentConfig);
            }
        }

        bool IsConfigurationFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return filePath.EndsWith(FileExtensions.ConfigurationFileExtension, StringComparison.OrdinalIgnoreCase);
        }
    }
}
