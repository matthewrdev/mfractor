using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Maui.Data.Models;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// The dynamic resource resolver resolves the dynamic resources that are available with a project.
    /// </summary>
    public interface IDynamicResourceResolver
    {
        /// <summary>
        /// Get's the <see cref="DynamicResourceDefinition"/>'s that are available to the provided <paramref name="filePath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The all available resources async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        IReadOnlyList<DynamicResourceResult> GetAvailableDynamicResources(Project project, IXamlPlatform platform, string filePath);

        /// <summary>
        /// Get's the <see cref="DynamicResourceDefinition"/>'s that are available in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The all available resources async.</returns>
        /// <param name="project">Project.</param>
        IReadOnlyList<DynamicResourceResult>  GetAvailableDynamicResources(Project project);

        /// <summary>
        /// Get's the <see cref="DynamicResourceDefinition"/>'s that are available to the provided <paramref name="context"/>.
        /// </summary>
        /// <returns>The all available resources async.</returns>
        /// <param name="context">Context.</param>
        IReadOnlyList<DynamicResourceResult>  GetAvailableDynamicResources(IXamlFeatureContext context);

        /// <summary>
        /// Get's the <see cref="DynamicResourceDefinition"/>'s that are available to the provided <paramref name="namedType"/>.
        /// </summary>
        /// <returns>The dynamic resources.</returns>
        IReadOnlyList<DynamicResourceResult>  GetAvailableDynamicResources(Project project, IXamlPlatform platform, INamedTypeSymbol namedType, bool includeApplicationResources = true);

        /// <summary>
        /// Finds the named dynamic resources async.
        /// </summary>
        /// <returns>The named static resources async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="resourceName">Resource name.</param>
        IReadOnlyList<DynamicResourceResult> FindNamedDynamicResources(Project project, string resourceName);

        /// <summary>
        /// Finds the named dynamic resources async.
        /// </summary>
        /// <returns>The named static resources async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="xamlFilePath">File path.</param>
        /// <param name="resourceName">Resource name.</param>
        IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(Project project, IXamlPlatform platform, string xamlFilePath, string resourceName);

        /// <summary>
        /// Finds the named dynamic resources async.
        /// </summary>
        /// <returns>The named static resources async.</returns>
        /// <param name="context">Context.</param>
        /// <param name="resourceName">Resource name.</param>
        IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(IXamlFeatureContext context, string resourceName);

        /// <summary>
        /// Finds the named dynamic resources async.
        /// </summary>
        /// <returns>The named static resources async.</returns>
        /// <param name="resourceName">Resource name.</param>
        IReadOnlyList<DynamicResourceResult> FindAvailableNamedDynamicResources(Project project, IXamlPlatform platform, INamedTypeSymbol namedType, string resourceName);
    }
}
