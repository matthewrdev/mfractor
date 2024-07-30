using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.Configuration;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.CustomRenderers
{
    /// <summary>
    /// Generates a custom renderer for a given type, base class and project.
    /// </summary>
    public interface ICustomRendererGenerator : ICodeGenerator
    {
        /// <summary>
        /// The configuration for creating iOS customer renderers.
        /// </summary>
        /// <value>The IOSC ustom renderer configuration.</value>
        IIOSCustomRendererConfiguration IOSCustomRendererConfiguration { get; set; }

        /// <summary>
        /// The configuration for creating Android custom renderers.
        /// </summary>
        /// <value>The android custom renderer configuration.</value>
        IAndroidCustomRendererConfiguration AndroidCustomRendererConfiguration { get; set; }

        /// <summary>
        /// The folder to place new custom renderers into.
        /// </summary>
        /// <value>The renderers folder.</value>
        string RenderersFolder { get; set; }

        /// <summary>
        /// For the given <paramref name="type"/>, can the <see cref="ICustomRendererGenerator"/> create a custom renderer?
        /// </summary>
        bool CanGenerateCustomRendererForType(INamedTypeSymbol type, IXamlPlatform platform);

        /// <summary>
        /// Generates a series of custom renderers for the given <paramref name="type"/> in the mobile projects that use <paramref name="targetProject"/>.
        /// </summary>
        IReadOnlyList<IWorkUnit> Generate(INamedTypeSymbol type, INamedTypeSymbol baseType, Project targetProject, IXamlPlatform platform, string folderPath, string rendererName);
    }
}
