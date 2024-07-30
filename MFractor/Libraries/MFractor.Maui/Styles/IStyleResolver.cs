using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MFractor.Maui.Data.Models;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui.Styles
{
    /// <summary>
    /// The style resolver is used to 
    /// </summary>
    public interface IStyleResolver
    {
        /// <summary>
        /// Creates a new <see cref="IStyle"/> from the given <paramref name="styleDefinition"/> and <paramref name="project"/>.
        /// </summary>
        /// <param name="styleDefinition"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        IStyle CreateStyle(StyleDefinition styleDefinition, Project project, IXamlPlatform platform);

        /// <summary>
        /// Resolves all <see cref="IStyle"/>'s that are available in the given <paramref name="filePath"/> and <paramref name="project"/>.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        IEnumerable<IStyle> ResolveAvailableStyles(Project project, IXamlPlatform platform, string filePath);

        /// <summary>
        /// Resolves the given style for 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="filePath"></param>
        /// <param name="styleName"></param>
        /// <returns></returns>
        IStyle ResolveStyleByName(Project project, IXamlPlatform platform, string filePath, string styleName);

        IEnumerable<IStyle> ResolveStylesByTargetType(Project project, IXamlPlatform platform, string filePath, INamedTypeSymbol targetType);
    }
}
