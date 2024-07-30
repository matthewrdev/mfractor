using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Fonts
{
    /// <summary>
    /// Resolves the <see cref="IEmbeddedFont"/>'s that are defined within a project.
    /// <para/>
    /// See: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/text/fonts
    /// </summary>
    public interface IEmbeddedFontsResolver
    {
        /// <summary>
        /// Resolves the <see cref="IEmbeddedFont"/>'s defined by the <paramref name="project"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEnumerable<IEmbeddedFont> GetEmbeddedFonts(Project project, IXamlPlatform platform);

        /// <summary>
        /// Resolves the <see cref="IEmbeddedFont"/> named <paramref name="name"/> defined by the <paramref name="project"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        IEmbeddedFont GetEmbeddedFontByName(Project project, IXamlPlatform platform, string name);
    }
}