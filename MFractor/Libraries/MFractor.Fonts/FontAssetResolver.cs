using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Fonts.Data.Repositories;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontAssetResolver))]
    class FontAssetResolver : IFontAssetResolver
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        public IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        [ImportingConstructor]
        public FontAssetResolver(Lazy<IProjectService> projectService,
                                 Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine)
        {
            this.projectService = projectService;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public IReadOnlyList<IFontAsset> GetAvailableFontAssets(Project project, bool searchReferences = true)
        {
            if (project == null)
            {
                return Array.Empty<IFontAsset>();
            }

            var results = new List<IFontAsset>();

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database.IsValid)
            {
                var fontRepo = database.GetRepository<FontFileAssetRepository>();
                var projectFileRepo = database.GetRepository<ProjectFileRepository>();

                var fontAssets = fontRepo.GetAllFonts();

                foreach (var asset in fontAssets)
                {
                    var pf = projectFileRepo.GetProjectFileFor(asset);
                    var font = new FontAsset(pf.FilePath, asset.Name, asset.FullName, asset.Style, asset.PostscriptName, asset.FamilyName, asset.IsWebFont, project);

                    results.Add(font);
                }
            }

            if (searchReferences)
            {
                var solution = project.Solution;

                var projects = solution.Projects.Where(p => p.IsMobileProject())
                                                .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id));

                foreach (var p in projects)
                {
                    var projectAssets = GetAvailableFontAssets(p, false);

                    if (projectAssets != null && projectAssets.Any())
                    {
                        results.AddRange(projectAssets.Where(pa => pa != null));
                    }
                }
            }

            return results;
        }

        public IReadOnlyList<IFontAsset> GetAvailableFontAssets(Solution solution)
        {
            if (solution == null)
            {
                return Array.Empty<IFontAsset>();
            }

            var result = new List<IFontAsset>();

            foreach (var project in solution.Projects)
            {
                if (!project.SupportsCompilation)
                {
                    continue;
                }

                var assets = GetAvailableFontAssets(project, false);

                if (assets != null && assets.Any())
                {
                    result.AddRange(assets);
                }
            }

            return result;

        }

        public IReadOnlyList<IFontAsset> GetAvailableFontAssets(ProjectIdentifier projectIdentifier, bool searchReferences = true)
        {
            var project = ProjectService.GetProject(projectIdentifier);

            return GetAvailableFontAssets(project, searchReferences);
        }

        public IReadOnlyList<IFontAsset> GetFontAssetsWithPostscriptName(Project project, string postscriptName, bool searchReferences = true)
        {
            if (project == null || string.IsNullOrEmpty(postscriptName))
            {
                return Array.Empty<IFontAsset>();
            }

            var results = new List<IFontAsset>();

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database.IsValid)
            {
                var fontRepo = database.GetRepository<FontFileAssetRepository>();
                var projectFileRepo = database.GetRepository<ProjectFileRepository>();

                var fontAssets = fontRepo.GetFontsWithPostscriptName(postscriptName);

                foreach (var asset in fontAssets)
                {
                    var pf = projectFileRepo.GetProjectFileFor(asset);
                    var font = new FontAsset(pf.FilePath, asset.Name, asset.FullName, asset.Style, asset.PostscriptName, asset.FamilyName, asset.IsWebFont, project);

                    results.Add(font);
                }
            }

            if (searchReferences)
            {
                var solution = project.Solution;

                var projects = solution.Projects.Where(p => p.IsMobileProject())
                                                .Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id));

                foreach (var p in projects)
                {
                    var projectAssets = GetFontAssetsWithPostscriptName(p, postscriptName, false);

                    if (projectAssets != null && projectAssets.Any())
                    {
                        results.AddRange(projectAssets.Where(pa => pa != null));
                    }
                }
            }

            return results;
        }

        public IReadOnlyList<IFontAsset> GetFontAssetsWithPostscriptName(ProjectIdentifier projectIdentifier, string postscriptName, bool searchReferences = true)
        {
            var project = ProjectService.GetProject(projectIdentifier);

            return GetFontAssetsWithPostscriptName(project, postscriptName, searchReferences);
        }

        public IFontAsset GetNamedFontAsset(Project project, string name)
        {
            if (project == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(project);

            if (database is null || !database.IsValid)
            {
                return null;
            }

            var fontRepo = database.GetRepository<FontFileAssetRepository>();
            var projectFileRepo = database.GetRepository<ProjectFileRepository>();

            var asset = fontRepo.GetFontFilAssetWithFileName(name);

            if (asset == null)
            {
                return null;
            }

            var projectFile = projectFileRepo.GetProjectFileFor(asset);

            if (projectFile == null)
            {
                return null;
            }

            var font = new FontAsset(projectFile.FilePath, asset.Name, asset.FullName, asset.Style, asset.PostscriptName, asset.FamilyName, asset.IsWebFont, project);

            return font;
        }
    }
}