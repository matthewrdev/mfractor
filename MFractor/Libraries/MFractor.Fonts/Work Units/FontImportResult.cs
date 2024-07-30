using System;
using System.Collections.Generic;
using MFractor.Utilities;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts.WorkUnits
{
    public class FontImportResult
    {
        public string SourceFontFile { get; }

        public string FontAssetName { get; }

        public IReadOnlyList<Project> Projects { get; }

        public IReadOnlyList<IProjectFile> DeclarationAddedFiles { get; }

        public IFont Font { get; }

        public string ResourceKey { get; }

        public FontImportResult(string sourceFontFile,
                                string fontAssetName,
                                IReadOnlyList<Project> projects,
                                IReadOnlyList<IProjectFile> declarationAddedFiles,
                                IFont font,
                                string resourceKey)
        {
            if (string.IsNullOrEmpty(sourceFontFile))
            {
                throw new ArgumentException("message", nameof(sourceFontFile));
            }

            if (string.IsNullOrEmpty(fontAssetName))
            {
                throw new ArgumentException("message", nameof(fontAssetName));
            }

            SourceFontFile = sourceFontFile;
            FontAssetName = fontAssetName;
            Projects = projects ?? throw new ArgumentNullException(nameof(projects));
            Font = font ?? throw new ArgumentNullException(nameof(font));
            ResourceKey = resourceKey;
            DeclarationAddedFiles = declarationAddedFiles ?? new List<IProjectFile>();
        }

        public string GetVirtualFilePath(Project project)
        {
            if (project == null)
            {
                return string.Empty;
            }

            if (project.IsAndroidProject())
            {
                return "Assets/" + FontAssetName;
            }

            if (project.IsAppleUnifiedProject())
            {
                return "Resources/" + FontAssetName;
            }

            return string.Empty;
        }
    }
}
