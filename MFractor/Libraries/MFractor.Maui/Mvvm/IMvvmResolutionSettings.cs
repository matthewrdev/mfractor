using System;
using MFractor.Configuration;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Mvvm
{
    /// <summary>
    /// The IMvvmResolutionSettings interface enables users to setup MFractor to resolve the MVVM relationship accross project boundaries.
    /// </summary>
    public interface IMvvmResolutionSettings : IConfigurable
    {
        /// <summary>
        /// What is the name of the project that the users XAML views reside within?
        /// </summary>
        /// <value>The name of the views project.</value>
        string ViewsProjectName { get; set; }

        /// <summary>
        /// What is the name of the project that the users view models reside within?
        /// </summary>
        /// <value>The name of the view models project.</value>
        string ViewModelsProjectName { get; set; }

        /// <summary>
        /// Get the project that contains the view models for the given project.
        /// </summary>
        /// <returns>The view models project.</returns>
        /// <param name="project">Project.</param>
        Project GetViewModelsProject(Project project);

        /// <summary>
        /// Get the project that contains the view models for the given project.
        /// </summary>
        /// <returns>The view models project.</returns>
        /// <param name="projectIdentifier">Project.</param>
        Project GetViewModelsProject(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Get the project that contain the XAML views for the given project.
        /// </summary>
        /// <returns>The views project.</returns>
        /// <param name="project">Project.</param>
        Project GetViewsProject(Project project);

        /// <summary>
        /// Get the project that contain the XAML views for the given project.
        /// </summary>
        /// <returns>The views project.</returns>
        /// <param name="projectIdentifier">Project.</param>
        Project GetViewsProject(ProjectIdentifier projectIdentifier);
    }
}