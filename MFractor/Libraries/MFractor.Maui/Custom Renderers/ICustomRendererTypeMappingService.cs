using System;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CustomRenderers
{
    /// <summary>
    /// Custom renderer type mapping service.
    /// </summary>
    public interface ICustomRendererTypeMappingService
    {
        /// <summary>
        /// Gets the fully qualified type of the custom renderer for the provided <paramref name="type"/> and <paramref name="platform"/>.
        /// </summary>
        string GetCustomRendererForType(INamedTypeSymbol type, IXamlPlatform platform, PlatformFramework platformFramework, bool backwardsCompatible, out string rendererControlType);
    }
}
