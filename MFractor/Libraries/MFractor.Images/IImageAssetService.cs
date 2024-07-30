using System;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MFractor.Images
{
    public interface IImageAssetService
    {
        /// <summary>
        /// Finds the image asset with the <paramref name="imageName"/> in the provided <paramref name="solution"/>.
        /// </summary>
        /// <returns>The image asset.</returns>
        /// <param name="imageName">Image name.</param>
        /// <param name="solution">The solution to serach.</param>
        IImageAsset FindImageAsset(string imageName, Solution solution);

        /// <summary>
        /// Finds the image asset with the <paramref name="imageName"/> in the provided <paramref name="projects"/>.
        /// </summary>
        /// <returns>The image asset.</returns>
        /// <param name="imageName">Image name.</param>
        /// <param name="projects">Projects.</param>
        IImageAsset FindImageAsset(string imageName, IReadOnlyList<Project> projects);

        /// <summary>
        /// Finds the image asset with the <paramref name="imageName"/> in the provided <paramref name="project"/>.
        /// </summary>
        /// <returns>The image asset.</returns>
        /// <param name="imageName">Image name.</param>
        /// <param name="project">Project.</param>
        /// <param name="searchDependantProjects">If set to <c>true</c> search dependant projects.</param>
        IImageAsset FindImageAsset(string imageName, Project project, bool searchDependantProjects = true);

        /// <summary>
        /// Gather all image assets in the given <paramref name="solution"/>, indexed by named.
        /// </summary>
        /// <param name="solution"></param>
        /// <returns></returns>
        IImageAssetCollection GatherImageAssets(Solution solution);

        /// <summary>
        /// Gather all image assets in the given <paramref name="projects"/>, indexed by named.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        IImageAssetCollection GatherImageAssets(IReadOnlyList<Project> projects);

        /// <summary>
        /// Gather all image assets in the given <paramref name="project"/>, indexed by named, and optionally searching any projects that depend on the <paramref name="project"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IImageAssetCollection GatherImageAssets(Project project, bool searchDependantProjects = false);
    }
}
