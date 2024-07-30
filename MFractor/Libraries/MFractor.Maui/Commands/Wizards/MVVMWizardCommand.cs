using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Commands;
using MFractor.Ide.Commands;
using MFractor.Maui.WorkUnits;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Commands.Wizards
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class MVVMWizardCommand : IWizardCommand
    {
        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public MVVMWizardCommand(Lazy<IWorkspaceService> workspaceService,
                                 Lazy<IWorkEngine> workEngine,
                                 Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
            this.xamlPlatforms = xamlPlatforms;
        }

        public void Execute(ICommandContext commandContext)
        {
            var targetProject = GetTargetProject(commandContext);

            if (targetProject is null)
            {
                IReadOnlyList<IWorkUnit> projectSelectionCallback(IReadOnlyList<Project> projects)
                {
                    LaunchMvvmWizard(projects.FirstOrDefault());

                    return Array.Empty<IWorkUnit>();
                }

                WorkEngine.ApplyAsync(new ProjectSelectorWorkUnit(GetAvailableProjects().ToList(),
                                                                  "Choose the project to add the new MVVM component into.",
                                                                  "Choose Target Project",
                                                                  projectSelectionCallback)
                {
                    Mode = ProjectSelectorMode.Single,
                });
            }
            else
            {
                LaunchMvvmWizard(targetProject);
            }
        }

        void LaunchMvvmWizard(Project targetProject)
        {
            var platform = XamlPlatforms.ResolvePlatform(targetProject);
            WorkEngine.ApplyAsync(new MVVMWizardWorkUnit()
            {
                Platform = platform,
                TargetProject = targetProject.GetIdentifier(),
                Projects = GetAvailableProjects().Select(p => p.GetIdentifier()).ToList(),
            });
        }

        Project GetTargetProject(ICommandContext commandContext)
        {
            if (commandContext is ISolutionPadCommandContext solutionPadCommandContext)
            {
                if (solutionPadCommandContext.SelectedItem  is IProjectFolder folder)
                {
                    if (XamlPlatforms.CanResolvePlatform(folder.Project))
                    {
                        return folder.Project;
                    }
                }

                return null;
            }

            if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                return documentCommandContext.CompilationProject;
            }

            return default;
        }

        IEnumerable<Project> GetAvailableProjects()
        {
            if (WorkspaceService.CurrentWorkspace == null
                || WorkspaceService.CurrentWorkspace.CurrentSolution == null)
            {
                return Enumerable.Empty<Project>();
            }

            return WorkspaceService.CurrentWorkspace.CurrentSolution.Projects.Where(p => XamlPlatforms.CanResolvePlatform(p)).ToList();
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
                Label = "MVVM Wizard",
                Description = "Launch the MVVM Wizard to generate a new view and view model.",
            };
        }
    }
}
