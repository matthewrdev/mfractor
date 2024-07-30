using System;
using MFractor.Configuration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Configuration
{
    /// <summary>
    /// Users can use the <see cref="IAppXamlConfiguration"/> configuration to specify a different App.xaml file for their application.
    /// 
    /// Generally, the file is named 'App.xaml', however, sometimes users rename it.
    /// </summary>
    public interface IAppXamlConfiguration : IConfigurable
    {
        /// <summary>
        /// The name of the App.xaml file for the project.
        /// </summary>
        /// <value>The name of the app xaml file.</value>
        string AppXamlFileName { get; set; }

        /// <summary>
        /// Locates the App.xaml file in the provided <paramref name="project"/>.
        /// </summary>
        /// <returns>The app xaml file.</returns>
        /// <param name="project">Project.</param>
        IProjectFile ResolveAppXamlFile(Project project, IXamlPlatform platform);

        /// <summary>
        /// Locates the App.xaml file in the provided <paramref name="projectIdentifier"/>.
        /// </summary>
        /// <returns>The app xaml file.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        IProjectFile ResolveAppXamlFile(ProjectIdentifier projectIdentifier, IXamlPlatform platform);

        /// <summary>
        /// Locates the App.xaml file in the provided <paramref name="project"/>.
        /// </summary>
        /// <returns>The app xaml file.</returns>
        /// <param name="project">Project.</param>
        IProjectFile ResolveAppXamlFile(Project project, string applicationXamlFileName);

        /// <summary>
        /// Locates the App.xaml file in the provided <paramref name="projectIdentifier"/>.
        /// </summary>
        /// <returns>The app xaml file.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        IProjectFile ResolveAppXamlFile(ProjectIdentifier projectIdentifier, string applicationXamlFileName);
    }
}
