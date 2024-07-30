using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageAssetService))]
    class ImageAssetService : IImageAssetService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        [ImportingConstructor]
        public ImageAssetService(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        public IImageAsset FindImageAsset(string imageName, Solution solution)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            return FindImageAsset(imageName, solution.GetMobileProjects());
        }

        public IImageAsset FindImageAsset(string imageName, IReadOnlyList<Project> projects)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                return null;
            }

            var assetName = Path.GetFileName(imageName);
            assetName = Path.GetFileNameWithoutExtension(assetName);

            var assets = GatherImageAssets(projects);

            assets.TryGetValue(assetName, out var imageAsset);

            return imageAsset;
        }

        /// <summary>
        /// Finds the image asset with the <paramref name="imageName"/> in the provided <paramref name="project"/>.
        /// </summary>
        /// <returns>The image asset.</returns>
        /// <param name="imageName">Image name.</param>
        /// <param name="project">Project.</param>
        /// <param name="searchDependantProjects">If set to <c>true</c> search dependant projects.</param>
        public IImageAsset FindImageAsset(string imageName, Project project, bool searchDependantProjects = true)
        {
            var solution = project.Solution;

            var projects = new List<Project>();

            if (searchDependantProjects)
            {
                projects.AddRange(solution.Projects.Where(p => p.IsMobileProject())
                                            .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id)));
            }

            if (project.IsMobileProject())
            {
                projects.Add(project);
            }

            return FindImageAsset(imageName, projects.Distinct().ToList());
        }

        /// <summary>
        /// Gathers all mobile image assets in the provide <paramref name="project"/> and, optionally, from any upstream mobile projects
        /// </summary>
        /// <returns>The image assets.</returns>
        /// <param name="project">Project.</param>
        /// <param name="searchDependantProjects">If set to <c>true</c> search dependant projects.</param>
        public IImageAssetCollection GatherImageAssets(Project project, bool searchDependantProjects = false)
        {
            var projects = new List<Project>();

            if (searchDependantProjects)
            {
                var solution = project.Solution;
                projects.AddRange(solution.Projects.Where(p => p.IsMobileProject())
                                            .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id)));
            }

            projects.Add(project);

            return GatherImageAssets(projects.Distinct().ToList());
        }

        /// <summary>
        /// Gathers all mobile image assets in the provide <paramref name="solution"/>.
        /// </summary>
        /// <returns>The image assets.</returns>
        /// <param name="solution">Projects.</param>
        public IImageAssetCollection GatherImageAssets(Solution solution)
        {
            if (solution == null)
            {
                return ImageAssetCollection.Empty;
            }

            var availableProjects = solution.GetMobileProjects();

            var nonMobileProjects = solution.Projects.Except(availableProjects);

            foreach (var p in nonMobileProjects)
            {
                if (!p.SupportsCompilation)
                {
                    continue;
                }

                if (p.IsMauiProject())
                { 
                    availableProjects.Add(p);
                }
            }

            return GatherImageAssets(availableProjects.Distinct().ToList());
        }

        /// <summary>
        /// Gathers all mobile image assets in the provide <paramref name="projects"/>.
        /// </summary>
        /// <returns>The image assets.</returns>
        /// <param name="projects">Projects.</param>
        public IImageAssetCollection GatherImageAssets(IReadOnlyList<Project> projects)
        {
            if (projects == null)
            {
                return ImageAssetCollection.Empty;
            }

            var imageAssets = new Dictionary<string, ImageAsset>();

            foreach (var project in projects)
            {
                if (project == null
                    || !project.SupportsCompilation)
                {
                    continue;
                }

                var projectFiles = ProjectService.GetProjectFiles(project);

                if (project.IsMauiProject())
                {
                    GatherMauiImages(imageAssets, project, projectFiles);
                }

                if (project.IsAndroidProject())
                {
                    GatherAndroidImageResources(imageAssets, project, projectFiles);
                }
                else if (project.IsAppleUnifiedProject())
                {
                    GatherAppleUnifiedImageResources(imageAssets, project, projectFiles);

                    GatherAppleUnifiedImageSets(imageAssets, project, projectFiles);

                    GatherAppleUnifiedAppIconSets(imageAssets, project, projectFiles);
                }
                else if (project.IsUWPProject())
                {
                    GatherUWPImageResources(imageAssets, project, projectFiles);
                }
                else
                {
                    GatherEmbeddedResourceImages(imageAssets, project, projectFiles);
                }
            }

            return new ImageAssetCollection(imageAssets);
        }

        public void GatherUWPImageResources(Dictionary<string, ImageAsset> imageAssets, Project project, IEnumerable<IProjectFile> projectFiles)
        {
            var images = projectFiles.Where(ImageAssetHelper.IsUWPImageResource);

            foreach (var image in images)
            {
                var imageName = Path.GetFileName(image.FilePath);
                var assetName = ImageNameHelper.GetUwpImageName(imageName);

                var extension = Path.GetExtension(image.FilePath);

                if (ImageHelper.IsImageFileExtension(extension))
                {
                    if (!imageAssets.ContainsKey(assetName))
                    {
                        imageAssets[assetName] = new ImageAsset(assetName, extension);
                    }

                    imageAssets[assetName].Add(project, image);
                }
            }
        }

        /// <summary>
        /// Gathers the ResizetizerNT shared images from the given
        /// </summary>
        /// <param name="imageAssets"></param>
        /// <param name="compilationProject"></param>
        /// <param name="projectFiles"></param>
        public void GatherEmbeddedResourceImages(Dictionary<string, ImageAsset> imageAssets, Project compilationProject, IEnumerable<IProjectFile> projectFiles)
        {
            var images = projectFiles.Where(ImageAssetHelper.IsEmbeddedResourceImageAsset);

            foreach (var image in images)
            {
                var imageName = Path.GetFileName(image.FilePath);
                var assetName = Path.GetFileNameWithoutExtension(imageName);
                var extension = Path.GetExtension(image.FilePath);

                if (ImageHelper.IsImageFileExtension(extension))
                {
                    if (!imageAssets.ContainsKey(assetName))
                    {
                        imageAssets[assetName] = new ImageAsset(assetName, extension);
                    }

                    imageAssets[assetName].Add(compilationProject, image);
                }
            }
        }


        /// <summary>
        /// Gathers the .NET Maui shared images from the given projects
        /// </summary>
        /// <param name="imageAssets"></param>
        /// <param name="compilationProject"></param>
        /// <param name="projectFiles"></param>
        public void GatherMauiImages(Dictionary<string, ImageAsset> imageAssets, Project compilationProject, IEnumerable<IProjectFile> projectFiles)
        {
            var images = projectFiles.Where(f => ImageAssetHelper.IsMauiImage(f) || ImageAssetHelper.IsMauiAppIcon(f));

            foreach (var image in images)
            {
                var imageName = Path.GetFileName(image.FilePath);
                var assetName = Path.GetFileNameWithoutExtension(imageName);
                var extension = Path.GetExtension(image.FilePath);

                if (ImageHelper.IsImageFileExtension(extension))
                {
                    if (!imageAssets.ContainsKey(assetName))
                    {
                        imageAssets[assetName] = new ImageAsset(assetName, extension);
                    }

                    imageAssets[assetName].Add(compilationProject, image);
                }
            }
        }

        public void GatherAndroidImageResources(Dictionary<string, ImageAsset> imageAssets, Project compilationProject, IEnumerable<IProjectFile> projectFiles)
        {
            var images = projectFiles.Where(ImageAssetHelper.IsAndroidImageResource);

            foreach (var image in images)
            {
                var imageName = Path.GetFileName(image.FilePath);
                var assetName = Path.GetFileNameWithoutExtension(imageName);
                var extension = Path.GetExtension(image.FilePath);

                if (ImageHelper.IsImageFileExtension(extension))
                {
                    if (!imageAssets.ContainsKey(assetName))
                    {
                        imageAssets[assetName] = new ImageAsset(assetName, extension);
                    }

                    imageAssets[assetName].Add(compilationProject, image);
                }
                else if (ImageAssetHelper.IsAndroidDrawableXmlImageResource(image))
                {
                    if (!imageAssets.ContainsKey(assetName))
                    {
                        imageAssets[assetName] = new ImageAsset(assetName, extension);
                    }

                    imageAssets[assetName].Add(compilationProject, image);
                }
            }
        }

        void GatherAppleUnifiedImageResources(Dictionary<string, ImageAsset> imageAssets,
                                              Project project,
                                              IEnumerable<IProjectFile> projectFiles)
        {
            var images = projectFiles.Where(ImageAssetHelper.IsAppleUnifiedImageResource);

            if (images.Any())
            {
                foreach (var image in images)
                {
                    var name = ImageNameHelper.GetBundleResourceImageName(image);

                    if (!imageAssets.ContainsKey(name))
                    {
                        imageAssets[name] = new ImageAsset(name, Path.GetExtension(image.FilePath));
                    }

                    imageAssets[name].Add(project, image);
                }
            }
        }

        public void GatherAppleUnifiedImageSets(Dictionary<string, ImageAsset> imageAssets,
                                                Project project,
                                                IEnumerable<IProjectFile> projectFiles,
                                                Func<IProjectFile, bool> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var imageCatalogs = projectFiles.Where(filter);

            if (imageCatalogs.Any())
            {
                foreach (var catalog in imageCatalogs)
                {
                    var name = ImageNameHelper.GetImageCatalogName(catalog, string.Empty);

                    var directory = Path.GetDirectoryName(catalog.FilePath);

                    var hasAsset = false;
                    if (Directory.Exists(directory))
                    {
                        try
                        {
                            var files = Directory.GetFiles(directory);

                            foreach (var file in files)
                            {
                                if (file != catalog.FilePath)
                                {
                                    if (!imageAssets.ContainsKey(name))
                                    {
                                        imageAssets[name] = new ImageAsset(name, string.Empty);
                                    }

                                    hasAsset = true;
                                    imageAssets[name].Add(project, new ImageCatalogAssetProjectFile(file, catalog));
                                }
                            }
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {

                        }
                    }

                    if (hasAsset)
                    {
                        imageAssets[name].Add(project, catalog);
                    }
                }
            }
        }

        public void GatherAppleUnifiedImageSets(Dictionary<string, ImageAsset> imageAssets, Project project, IEnumerable<IProjectFile> projectFiles)
        {
            GatherAppleUnifiedImageSets(imageAssets, project, projectFiles, ImageAssetHelper.IsAppleUnifiedImageSet);
        }

        public void GatherAppleUnifiedAppIconSets(Dictionary<string, ImageAsset> imageAssets, Project project, IEnumerable<IProjectFile> projectFiles)
        {
            GatherAppleUnifiedImageSets(imageAssets, project, projectFiles, ImageAssetHelper.IsAppleUnifiedAppIconSet);
        }
    }
}