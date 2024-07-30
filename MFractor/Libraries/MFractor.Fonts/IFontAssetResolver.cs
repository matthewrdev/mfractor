using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    /// <summary>
    /// Resolves the available <see cref="IFontAsset"/>'s for a given project or assembly.
    /// </summary>
    public interface IFontAssetResolver
    {
        IFontAsset GetNamedFontAsset(Project project, string name);

        IReadOnlyList<IFontAsset> GetAvailableFontAssets(Project project, bool searchReferences = true);

        IReadOnlyList<IFontAsset> GetAvailableFontAssets(ProjectIdentifier projectIdentifier, bool searchReferences = true);

        IReadOnlyList<IFontAsset> GetAvailableFontAssets(Solution solution);

        IReadOnlyList<IFontAsset> GetFontAssetsWithPostscriptName(Project project, string postscriptName, bool searchReferences = true);

        IReadOnlyList<IFontAsset> GetFontAssetsWithPostscriptName(ProjectIdentifier projectIdentifier, string postscriptName, bool searchReferences = true);
    }
}
