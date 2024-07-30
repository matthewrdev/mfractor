using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Commands;
using MFractor.Maui.Configuration;
using MFractor.Maui.WorkUnits;
using MFractor.Ide.Commands;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Commands.Wizards
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class ValueConverterWizardCommand : IWizardCommand
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        readonly Lazy<IWorkEngine> workEngine;
        readonly Lazy<IProjectService> projectService;
        readonly Lazy<IAppXamlConfiguration> appXamlConfiguration;
        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;

        public IWorkspaceService WorkspaceService => workspaceService.Value;
        public IWorkEngine WorkEngine => workEngine.Value;
        public IAppXamlConfiguration AppXamlConfiguration => appXamlConfiguration.Value;
        public IProjectService ProjectService => projectService.Value;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public ValueConverterWizardCommand(Lazy<IWorkspaceService> workspaceService,
                                           Lazy<IWorkEngine> workEngine,
                                           Lazy<IProjectService> projectService,
                                           Lazy<IAppXamlConfiguration> appXamlConfiguration,
                                           Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
            this.projectService = projectService;
            this.appXamlConfiguration = appXamlConfiguration;
            this.xamlPlatforms = xamlPlatforms;
        }

        IEnumerable<Project> GetAvailableProjects()
        {
            if (WorkspaceService.CurrentWorkspace == null
                || WorkspaceService.CurrentWorkspace.CurrentSolution == null)
            {
                return Enumerable.Empty<Project>();
            }

            return WorkspaceService.CurrentWorkspace.CurrentSolution.Projects.Where(p => XamlPlatforms.CanResolvePlatform(p))
                                                                              .ToList();
        }

        public void Execute(ICommandContext commandContext)
        {
            var targetProject = GetTargetProject(commandContext);

            if (targetProject is null)
            {
                IReadOnlyList<IWorkUnit> projectSelectionCallback(IReadOnlyList<Project> projects)
                {
                    LaunchValueConverterWizard(commandContext, projects.FirstOrDefault());

                    return Array.Empty<IWorkUnit>();
                }

                WorkEngine.ApplyAsync(new ProjectSelectorWorkUnit(GetAvailableProjects().ToList(),
                                                                  "Choose the project to add the new value converter to.",
                                                                  "Choose Target Project",
                                                                  projectSelectionCallback)
                {
                    Mode = ProjectSelectorMode.Single,
                });
            }
            else
            {
                LaunchValueConverterWizard(commandContext, targetProject);
            }
        }

        void LaunchValueConverterWizard(ICommandContext commandContext, Project targetProject)
        {
            var targetFiles = GetTargetFiles(commandContext, targetProject).ToList();
            var platform = XamlPlatforms.ResolvePlatform(targetProject);

            WorkEngine.ApplyAsync(new ValueConverterWizardWorkUnit()
            {
                Platform = platform,
                TargetProject = targetProject.GetIdentifier(),
                TargetFiles = targetFiles,
                CreateXamlDeclaration = targetFiles.Any(),
            });
        }

        Project GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                if (solutionPadCommandContext.SelectedItem is IProjectFolder folder
                    && XamlPlatforms.CanResolvePlatform(folder.Project))
                {
                    return folder.Project;
                }

                return null;
            }

            if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                return documentCommandContext.CompilationProject;
            }

            return default;
        }

        IEnumerable<IProjectFile> GetTargetFiles(ICommandContext commandContext, Project project)
        {
            var platform = XamlPlatforms.ResolvePlatform(project);

            if (platform is null)
            {
                return Enumerable.Empty<IProjectFile>();
            }

            var files = new List<IProjectFile>();

            if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                var extension = Path.GetExtension(documentCommandContext.FilePath);
                if (extension == ".xaml")
                {
                    var projectFile = ProjectService.GetProjectFileWithFilePath(documentCommandContext.CompilationProject, documentCommandContext.FilePath);

                    if (projectFile != null)
                    {
                        files.Add(projectFile);
                    }
                }
            }

            var appXaml = AppXamlConfiguration.ResolveAppXamlFile(project, platform);

            if (appXaml != null)
            {
                files.Add(appXaml);
            }

            return files;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var projects = GetAvailableProjects();

            if (projects == null || !projects.Any())
            {
                return null;
            }

            return new CommandState()
            {
                Label = "Value Converter Wizard",
                Description = "The Value Converter Wizard can be used to create new value converters."
            };
        }
    }
}
