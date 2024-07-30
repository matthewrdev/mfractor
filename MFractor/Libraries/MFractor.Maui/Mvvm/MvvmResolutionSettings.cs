using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis;
using MFractor.Workspace;

namespace MFractor.Maui.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMvvmResolutionSettings))]
    class MvvmResolutionSettings : Configurable, IMvvmResolutionSettings
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ExportProperty("If the apps XAML views and view models are in separate projects, what is the name of project that contains the XAML views?")]
        public string ViewsProjectName
        {
            get; set;
        } = null;

        [ExportProperty("If the apps XAML views and view models are in separate projects, what is the name of project that contains the ViewModels?")]
        public string ViewModelsProjectName
        {
            get; set;
        } = null;

        public override string Identifier => "com.mfractor.configuration.xaml.mvvm_resolution";

        public override string Name => "MVVM Resolution Settings";

        public override string Documentation => "The MVVM resolution settings can be used to specify the project that your views or view models sit within. If you prefer to separate your views and view models into separate projects, these settings enable MFractor to resolve the MVVM relationship across project boundaries.";

        [ImportingConstructor]
        public MvvmResolutionSettings(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        public Project GetViewModelsProject(Project project)
        {
            if (string.IsNullOrEmpty(ViewModelsProjectName))
            {
                return project;
            }

            return project.Solution.Projects.FirstOrDefault(p => p.Name == ViewModelsProjectName) ?? project;
        }

        public Project GetViewsProject(Project project)
        {
            if (string.IsNullOrEmpty(ViewsProjectName))
            {
                return project;
            }

            return project.Solution.Projects.FirstOrDefault(p => p.Name == ViewsProjectName) ?? project;
        }

        public Project GetViewModelsProject(ProjectIdentifier projectIdentifier)
        {
            return GetViewModelsProject(ProjectService.GetProject(projectIdentifier));
        }

        public Project GetViewsProject(ProjectIdentifier projectIdentifier)
        {
            return GetViewModelsProject(ProjectService.GetProject(projectIdentifier));
        }
    }
}