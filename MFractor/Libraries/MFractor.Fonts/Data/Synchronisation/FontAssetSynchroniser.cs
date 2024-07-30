using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Workspace.Data.Synchronisation;
using MFractor.Fonts.Data.Repositories;
using Microsoft.CodeAnalysis;
using MFractor.Workspace.Data;
using MFractor.Workspace;
using MFractor.Workspace.Data.Repositories;
using System.Threading.Tasks;
using MFractor.Fonts.Data.Models;

namespace MFractor.Fonts.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FontAssetSynchroniser : IAssetResourceSynchroniser
    {
        static readonly string[] supportedFileExtensions = { ".otf", ".ttf" };

        readonly Lazy<IFontService> fontService;
        IFontService FontService => fontService.Value;

        public string[] SupportedFileExtensions => supportedFileExtensions;

        [ImportingConstructor]
        public FontAssetSynchroniser(Lazy<IFontService> fontService)
        {
            this.fontService = fontService;
        }

        public bool IsAvailable(Solution solution, Project project)
        {
            return true;
        }

        public Task<bool> CanSynchronise(Solution solution, Project project, IProjectFile projectFile)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> Synchronise(Solution solution, Project project, IProjectFile projectFile, IProjectResourcesDatabase database)
        {
            var projectFileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);

            if (projectFileModel == null)
            {
                return false;
            }

            var fontInformation = FontService.GetFont(projectFile.FilePath);

            if (fontInformation == null)
            {
                return false;
            }

            var fontAssetRepository = database.GetRepository<FontFileAssetRepository>();

            var fontAsset = new FontFileAsset();

            fontAsset.FullName = fontInformation.FullName;
            fontAsset.FileName = fontInformation.FileName;
            fontAsset.Name = fontInformation.Name;
            fontAsset.Style = fontInformation.Style;
            fontAsset.ProjectFileKey = projectFileModel.PrimaryKey;
            fontAsset.PostscriptName = fontInformation.PostscriptName;
            fontAsset.FamilyName = fontInformation.FamilyName;
            fontAsset.IsWebFont = fontInformation.IsWebFont;

            fontAssetRepository.Insert(fontAsset);

            return true;
        }
    }
}
