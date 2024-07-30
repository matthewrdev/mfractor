using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Images.Importing.Generators;
using MFractor.Images.Models;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IIconImporterService))]
    class IconImporterService : IIconImporterService
    {
        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IWorkspaceProjectService> projectUtils;
        IWorkspaceProjectService ProjectUtils => projectUtils.Value;

        readonly Lazy<IAndroidAdaptiveIconGenerator> androidAdaptiveIconGenerator;
        public IAndroidAdaptiveIconGenerator AndroidAdaptiveIconGenerator => androidAdaptiveIconGenerator.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<IImageUtilities> imageSizeUtilities;
        public IImageUtilities ImageSizeUtilities => imageSizeUtilities.Value;

        [ImportingConstructor]
        public IconImporterService(Lazy<IWorkspaceProjectService> projectUtils,
                                   Lazy<IProjectService> projectService,
                                   Lazy<IWorkEngine> workEngine,
                                   Lazy<IAndroidAdaptiveIconGenerator> androidAdaptiveIconGenerator,
                                   Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                   Lazy<IImageUtilities> imageSizeUtilities)
        {
            this.projectUtils = projectUtils;
            this.projectService = projectService;
            this.workEngine = workEngine;
            this.androidAdaptiveIconGenerator = androidAdaptiveIconGenerator;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.imageSizeUtilities = imageSizeUtilities;
        }

        public async Task CleanupAppIconAsync(Project targetProject)
        {
            // Ignored for the moment.
            return;

            if (targetProject.IsAppleUnifiedProject())
            {
                await CleanupAppIconFromIOSProjectAsync(targetProject);
            }
            else if (targetProject.IsAndroidProject())
            {
                await CleanupAppIconFromAndroidProjectAsync(targetProject);
            }
        }

        async Task CleanupAppIconFromIOSProjectAsync(Project project)
        {
            // The rules for cleaning up on iOS projects is simple - remove the folder AppIcon.appiconset from the
            //  Assets.xcassets catalog
            // Should check for the files in the folder if they exist on the project file.
            // A future implementation should check the Info.plist for existing

            // TODO: Parametrize the base path
            var appIconVirtualPath = Path.Combine("Assets.xcassets", "AppIcon.appiconset");
            var appIconDiskPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, appIconVirtualPath);

            if (Directory.Exists(appIconDiskPath))
            {
                var files = Directory.EnumerateFiles(appIconDiskPath);
                if (files.Any())
                {
                    await ProjectUtils.DeleteFilesAsync(project, files.ToArray());
                }
            }
        }

        async Task CleanupAppIconFromAndroidProjectAsync(Project project)
        {
            var mipmapPaths = GetMipmapPaths(project);      // TODO: Parametrize the base path
            var filesToDelete = new List<string>();

            foreach (var mipmapPath in mipmapPaths)
            {
                if (Directory.Exists(mipmapPath))
                {
                    // TODO: Need to check for recursion
                    // Usually the mipmap folder shouldn't have any subfolders, but we should prevent
                    var files = Directory.EnumerateFiles(mipmapPath);
                    if (files.Any())
                    {
                        filesToDelete.AddRange(files);
                    }
                }
            }

            await ProjectUtils.DeleteFilesAsync(project, filesToDelete.ToArray());
        }

        IEnumerable<string> GetMipmapPaths(Project project)
        {
            var basePath = "Resources";
            var paths = new List<string>();

            foreach (var scale in ImageScale.AndroidScales)
            {
                var virtualPath = Path.Combine(basePath, $"mipmap-{scale.Name}");
                var diskPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, virtualPath);
                paths.Add(diskPath);
            }

            return paths;
        }

        public async Task<bool> ImportIconAsync(IEnumerable<IconImage> icons, string sourceImagePath, Project targetProject)
        {
            var success = true;
            var workUnits = icons
                .Select(i => GetCreateImageFileWorkUnit(targetProject, i, sourceImagePath))
                .ToList();

            if (targetProject.IsAppleUnifiedProject())
            {
                workUnits.Add(GetAssetCatalogMetadataWorkUnit(targetProject, icons));
            }
            else if (targetProject.IsAndroidProject())
            {
                if (icons.Any(i => i.IsAdaptive))
                {
                    var iconRoundPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(targetProject, AndroidAdaptiveIconGenerator.AdaptiveIconRoundPath);
                    if (ProjectService.GetProjectFileWithFilePath(targetProject, iconRoundPath) == null)
                    {
                        var workUnit = AndroidAdaptiveIconGenerator.GenerateRoundIcon(targetProject, "launcher_foreground", "@android:color/white");
                        workUnits.Add(workUnit);
                    }

                    var iconPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(targetProject, AndroidAdaptiveIconGenerator.AdaptiveIconPath);
                    if (ProjectService.GetProjectFileWithFilePath(targetProject, iconPath) == null)
                    {
                        var workUnit = AndroidAdaptiveIconGenerator.GenerateIcon(targetProject, "launcher_foreground", "@android:color/white");
                        workUnits.Add(workUnit);
                    }
                }

                await UpdateManifestAppIcon(targetProject);
            }

            success &= await ProjectUtils.AddProjectFilesAsync(targetProject, workUnits);

            return success;
        }

        async Task UpdateManifestAppIcon(Project targetProject)
        {
            var manifest = ProjectService.FindProjectFile(targetProject, (filePath) => Path.GetFileName(filePath).Equals("AndroidManifest.xml", StringComparison.OrdinalIgnoreCase));

            if (manifest is null)
            {
                return;
            }

            var syntax = XmlSyntaxParser.ParseFile(manifest.FileInfo);

            if (syntax is null || syntax.Root is null)
            {
                return;
            }

            if (syntax.Root.Name.FullName != "manifest" || !syntax.Root.HasChildren)
            {
                return;
            }

            var application = syntax.Root.Children.FirstOrDefault(n => n.Name.FullName == "application");
            if (application is null)
            {
                return;
            }

            var iconAttribute = application.GetAttributeByName("android:icon");
            if (iconAttribute is null)
            {
                var workUnit = new InsertTextWorkUnit(" android:icon=\"@mipmap/icon\"", application.NameSpan.End, manifest.FilePath);
                await WorkEngine.ApplyAsync(workUnit);
            }
            else
            {
                var workUnit = new ReplaceTextWorkUnit(manifest.FilePath, "@mipmap/icon", iconAttribute.Value.Span);
                await WorkEngine.ApplyAsync(workUnit);
            }
        }

        public Task<bool> ImportIconAsync(IEnumerable<AppIconSet> iconSets, string sourceImagePath, Project targetProject)
        {
            var icons = iconSets.SelectMany(i => i.Images);
            return ImportIconAsync(icons, sourceImagePath, targetProject);
        }

        CreateProjectFileWorkUnit GetCreateImageFileWorkUnit(Project project, IconImage iconImage, string sourceImagePath)
        {
            var buildAction = GetBuildActionFromProjectType(project);
            var virtualPath = Path.Combine(iconImage.DestinationFolder, iconImage.FileName);
            var outputPath = VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, virtualPath);

            var workUnit = new CreateProjectFileWorkUnit
            {
                FilePath = outputPath,
                VirtualFilePath = virtualPath,
                TargetProject = project,
                IsBinary = true,
                BuildAction = buildAction,
                ShouldAddFoldersToMsBuild = false,
                WriteContentAction = (stream) =>
                {
                    ImageSizeUtilities.ResizeImage(sourceImagePath, iconImage.PixelWidth, iconImage.PixelHeight, stream);
                }
            };

            return workUnit;
        }

        CreateProjectFileWorkUnit GetAssetCatalogMetadataWorkUnit(Project targetProject, IEnumerable<IconImage> icons) => new CreateProjectFileWorkUnit
        {
            TargetProject = targetProject,
            FilePath = GetMetadataDestinationFileName(targetProject, icons),
            VirtualFilePath = GetMetadataVirtualPath(targetProject, icons),
            FileContent = GenerateImageSetMetadata(icons),
            BuildAction = "ImageAsset",
            ShouldOpen = false,
            Visible = false,
            ShouldAddFoldersToMsBuild = false,
        };

        string GenerateImageSetMetadata(IEnumerable<IconImage> icons)
        {
            var metadata = new IOSAssetCatalogMetadata();
            var images = icons
                .Select(i => new IOSImageSetEntry
                {
                    Filename = i.FileName,
                    Scale = i.Scale.Name,
                    Idiom = i.Idiom.GetImportName(),
                    Size = i.SizeDescription
                });

            metadata.Images.AddRange(images);
            return metadata.Serialize();
        }

        string GetMetadataDestinationFileName(Project project, IEnumerable<IconImage> icons) => 
            VirtualFilePathHelper.VirtualProjectPathToDiskPath(project, GetMetadataVirtualPath(project, icons));

        string GetMetadataVirtualPath(Project project, IEnumerable<IconImage> icons) => 
            Path.Combine(GetDestinationPath(icons), "Contents.json");

        string GetDestinationPath(IEnumerable<IconImage> icons) => icons.First().DestinationFolder;

        string GetBuildActionFromProjectType(Project project) =>
            project.IsAppleUnifiedProject()
            ? "ImageAsset"
            : "AndroidResource";
    }
}
