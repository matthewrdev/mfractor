using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Ide.Commands;
using MFractor.Localisation.WorkUnits;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Commands
{
    [RequiresLicense]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export]
    class LocalisationWizardCommand : IWizardCommand, IAnalyticsFeature
    {
        readonly Lazy<IFeatureContextFactoryRepository> featureContextFactoryRepository;
        public IFeatureContextFactoryRepository FeatureContextFactoryRepository => featureContextFactoryRepository.Value;

        readonly Lazy<ILocalisationService> localisationService;
        public ILocalisationService LocalisationService => localisationService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public string AnalyticsEvent => "Localisation Wizard";

        [ImportingConstructor]
        public LocalisationWizardCommand(Lazy<IFeatureContextFactoryRepository> featureContextFactoryRepository,
                                         Lazy<ILocalisationService> localisationService,
                                         Lazy<IWorkEngine> workEngine,
                                         Lazy<IProjectService> projectService)
        {
            this.featureContextFactoryRepository = featureContextFactoryRepository;
            this.localisationService = localisationService;
            this.workEngine = workEngine;
            this.projectService = projectService;
        }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext is IDocumentCommandContext documentCommandContext)
            {
                ExecuteFromActiveDocument(documentCommandContext);
            }
        }

        void ExecuteFromActiveDocument(IDocumentCommandContext documentCommandContext)
        {
            var factory = FeatureContextFactoryRepository.GetInterestedFeatureContextFactory(documentCommandContext.CompilationProject, documentCommandContext.FilePath);

            var context = factory.CreateFeatureContext(documentCommandContext.CompilationProject, documentCommandContext.FilePath, 0);

            var project = documentCommandContext.CompilationProject;

            var workUnit = new LocaliseDocumentWorkUnit(context.Document, project, GetTargetProjects(project), context.SemanticModel);

            WorkEngine.ApplyAsync(workUnit);
        }

        IReadOnlyList<Project> GetTargetProjects(Project project)
        {
            var projects = new List<Project>();

            foreach (var projectReference in project.ProjectReferences)
            {
                var target = project.Solution.GetProject(projectReference.ProjectId);

                if (target != null)
                {
                    var resxFiles = ProjectService.GetProjectFilesWithExtension(target, ".resx");

                    if (resxFiles != null && resxFiles.Any())
                    {
                        projects.Add(target);
                    }
                }
            }

            return projects;
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var documentCommandContext = commandContext as IDocumentCommandContext;
            if (documentCommandContext is null)
            {
                return default;
            }

            return GetDefaultExecutionState(documentCommandContext);
        }

        CommandState GetDefaultExecutionState(IDocumentCommandContext documentCommandContext)
        {
            if (!LocalisationService.CanLocalise(documentCommandContext.CompilationProject, documentCommandContext.FilePath))
            {
                return default;
            }

            return new CommandState(true, true, "Localize Document", "Open the localization wizard to walk through the strings in this document and migrate them into a i18n file for this platform.");
        }
    }
}
