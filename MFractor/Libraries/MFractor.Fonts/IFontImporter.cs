using System;
using System.Collections.Generic;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    /// <summary>
    /// The font importer adds a font asset into a given <see cref="Project"/>, setting up any necessary infrastructure required to correctly register that font.
    /// </summary>
    public interface IFontImporter
    {
        /// <summary>
        /// Import the given <paramref name="fontFilePath"/> into the <paramref name="project"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="fontFilePath"></param>
        /// <param name="fontAssetName"></param>
        /// <returns></returns>
        IReadOnlyList<IWorkUnit> ImportFont(Project project, string fontFilePath, string fontAssetName);

        /// <summary>
        /// Imports the <paramref name="fontFilePath"/> into the given Android <paramref name="project"/>.
        /// </summary>
        IReadOnlyList<IWorkUnit> ImportFontIntoAndroid(Project project, string fontFilePath, string fontAssetName);

        /// <summary>
        /// Imports the <paramref name="fontFilePath"/> into the given iOS <paramref name="project"/>.
        /// </summary>
        IReadOnlyList<IWorkUnit> ImportFontIntoIOS(Project project, string fontFilePath, string fontAssetName);

        /// <summary>
        /// Imports the <paramref name="fontFilePath"/> into the given UWP <paramref name="project"/>.
        /// </summary>
        IReadOnlyList<IWorkUnit> ImportFontIntoUWP(Project project, string fontFilePath, string fontAssetName);
    }
}
