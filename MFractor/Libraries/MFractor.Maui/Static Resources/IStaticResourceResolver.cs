using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.StaticResources
{
    /// <summary>
    /// The static resource resolver resolves the static resources that are available with a project.
    /// </summary>
    public interface IStaticResourceResolver
    {
        /// <summary>
        /// Gets all available resources for the provided <paramref name="filePath"/> in the <paramref name="project"/>.
        /// </summary>
        /// <returns>The all available resources async.</returns>
        /// <param name="project">Project.</param>
        /// <param name="filePath">File path.</param>
        /// <param name="includeAppplicationResources">If set to <c>true</c> include appplication resources.</param>
        IStaticResourceCollection GetAvailableResources(Project project, IXamlPlatform platform, string filePath, bool includeAppplicationResources = true);

        IStaticResourceCollection FindNamedStaticResources(Project compilationProject,
                                                           IXamlPlatform platform,
                                                           string filePath,
                                                           string resourceName,
                                                           bool searchApplicationResources = true,
                                                           bool searchBaseClasses = true);
    }
}
