using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;
using MFractor.Maui.Mvvm;
using MFractor;
using MFractor.Workspace;

namespace MFractor.Maui.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBaseViewModelInferenceService))]
    class BaseViewModelInferenceService : IBaseViewModelInferenceService
    {
        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public BaseViewModelInferenceService(Lazy<IMvvmResolver> mvvmResolver,
                                             Lazy<IProjectService> projectService)
        {
            this.mvvmResolver = mvvmResolver;
            this.projectService = projectService;
        }

        public Task<INamedTypeSymbol> InferBaseViewModelForProjectAsync(Project project)
        {
            return Task.Run(() =>
           {
               return InferBaseViewModelForProject(project);
           });
        }

        public async Task<INamedTypeSymbol> InferBaseViewModelForProjectAsync(ProjectIdentifier projectIdentifier)
        {
            var project = ProjectService.GetProject(projectIdentifier);

            if (project == null)
            {
                return null;
            }

            return await InferBaseViewModelForProjectAsync(project);
        }

        public INamedTypeSymbol InferBaseViewModelForProject(Project project)
        {
            if (project == null)
            {
                return null;
            }

            var xamlFiles = ProjectService.GetProjectFilesWithExtension(project, ".xaml");

            var pages = xamlFiles.Where(xf => xf.Name.EndsWith("Page.xaml", StringComparison.OrdinalIgnoreCase));

            if (!pages.Any())
            {
                return null;
            }

            var matches = new Dictionary<INamedTypeSymbol, int>();

            foreach (var page in pages)
            {
                var viewModel = MvvmResolver.ResolveViewModelSymbol(project, page.FilePath);

                if (viewModel != null)
                {
                    var baseType = viewModel.BaseType;

                    if (baseType.SpecialType != SpecialType.None)
                    {
                        if (viewModel.Interfaces.Any())
                        {
                            baseType = viewModel.Interfaces.FirstOrDefault();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (matches.ContainsKey(baseType))
                    {
                        matches[baseType]++;
                    }
                    else
                    {
                        matches[baseType] = 1;
                    }
                }
            }

            var max = 0;
            INamedTypeSymbol candidate = null;
            foreach (var match in matches)
            {
                if (match.Value > max)
                {
                    candidate = match.Key;
                }
            }

            return candidate;
        }

        public INamedTypeSymbol InferBaseViewModelForProject(ProjectIdentifier projectIdentifier)
        {
            var project = ProjectService.GetProject(projectIdentifier);

            if (project == null)
            {
                return null;
            }

            return InferBaseViewModelForProject(project);
        }
    }
}