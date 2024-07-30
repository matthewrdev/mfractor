using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Fonts;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Fonts
{
    /// <summary>
    /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
    /// </summary>
    public interface IFontFamilyOnPlatformGenerator : ICodeGenerator
    {
        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="font">Font.</param>
        /// <param name="resourceKey">Key.</param>
        /// <param name="projects">Projects.</param>
        /// <param name="childIndent">Child indent.</param>
        string GenerateXaml(IFont font,
                          string resourceKey,
                          IEnumerable<Project> projects,
                          string childIndent = "  ");

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="font">Font.</param>
        /// <param name="resourceKey">Key.</param>
        /// <param name="platforms">Projects.</param>
        /// <param name="childIndent">Child indent.</param>
        string GenerateXaml(IFont font,
                          string resourceKey,
                          IEnumerable<PlatformFramework> platforms,
                          string childIndent = "  ");

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="fontFileName">Font file path.</param>
        /// <param name="fontName">Font name.</param>
        /// <param name="postscriptName">Postscript name.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="projects">Projects.</param>
        /// <param name="childIndent">Child indent.</param>
        string GenerateXaml(string fontFileName,
                          string fontName,
                          string postscriptName,
                          string fontStyle,
                          string resourceKey,
                          string typographicFamilyName,
                          IEnumerable<Project> projects,
                          string childIndent = "  ");


        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="fontFileName">Font file path.</param>
        /// <param name="fontName">Font name.</param>
        /// <param name="postscriptName">Postscript name.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="platforms">Platforms.</param>
        /// <param name="childIndent">Child indent.</param>
        string GenerateXaml(string fontFileName,
                          string fontName,
                          string postscriptName,
                          string fontStyle,
                          string resourceKey,
                          string typographicFamilyName,
                          IEnumerable<PlatformFramework> platforms,
                          string childIndent = "  ");

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="font">Font.</param>
        /// <param name="resourceKey">Key.</param>
        /// <param name="projects">Projects.</param>
        XmlNode Generate(IFont font,
                       string resourceKey,
                       IEnumerable<Project> projects);

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="font">Font.</param>
        /// <param name="resourceKey">Key.</param>
        /// <param name="platforms">Projects.</param>
        XmlNode Generate(IFont font,
                       string resourceKey,
                       IEnumerable<PlatformFramework> platforms);

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="fontFileName">Font file path.</param>
        /// <param name="fontName">Font name.</param>
        /// <param name="postscriptName">Postscript name.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="projects">Projects.</param>
        XmlNode Generate(string fontFileName,
                       string fontName,
                       string postscriptName,
                       string fontStyle,
                       string resourceKey,
                          string typographicFamilyName,
                       IEnumerable<Project> projects);

        /// <summary>
        /// Generates the XAML OnPlatform code needed to reference apply a font in XAML.
        /// </summary>
        /// <returns>The xaml.</returns>
        /// <param name="fontFileName">Font file path.</param>
        /// <param name="fontName">Font name.</param>
        /// <param name="postscriptName">Postscript name.</param>
        /// <param name="fontStyle">Font style.</param>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="platforms">Projects.</param>
        XmlNode Generate(string fontFileName,
                         string fontName,
                         string postscriptName,
                         string fontStyle,
                         string resourceKey,
                          string typographicFamilyName,
                         IEnumerable<PlatformFramework> platforms);

    }
}
