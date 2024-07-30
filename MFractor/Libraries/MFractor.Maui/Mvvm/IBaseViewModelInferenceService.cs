using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Mvvm
{
    /// <summary>
    /// Inspects a project and locates the <see cref="INamedTypeSymbol"/> that is most likely to be the default base class for new ViewModels.
    /// </summary>
    public interface IBaseViewModelInferenceService
    {
        /// <summary>
        /// Given the <paramref name="project"/>, deduce the most likely base class for new ViewModels.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        INamedTypeSymbol InferBaseViewModelForProject(Project project);

        /// <summary>
        /// Given the <paramref name="projectIdentifier"/>, deduce the most likely base class for new ViewModels.
        /// </summary>
        /// <param name="projectIdentifier"></param>
        /// <returns></returns>
        INamedTypeSymbol InferBaseViewModelForProject(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Given the <paramref name="project"/>, asynchronously deduces the most likely base class for new ViewModels.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task<INamedTypeSymbol> InferBaseViewModelForProjectAsync(Project project);

        /// <summary>
        /// Given the <paramref name="projectIdentifier"/>, asynchronously deduces the most likely base class for new ViewModels.
        /// </summary>
        /// <param name="projectIdentifier"></param>
        /// <returns></returns>
        Task<INamedTypeSymbol> InferBaseViewModelForProjectAsync(ProjectIdentifier projectIdentifier);
    }
}
