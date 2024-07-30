using System;
using MFractor.Fonts;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Workspace;
using MFractor.Xml;

namespace MFractor.Maui.Fonts
{
    /// <summary>
    /// The <see cref="IFontFamilyResolver"/> can resolve the <see cref="IFont"/> for a given xml node.
    /// <para/>
    /// The resolver supports resolving fonts through:
    /// <para/>
    ///  * FontFamily attributes that use a static resource expressions to point to an OnPlatform.
    /// <para/>
    ///  * Styles that specify the font family.
    /// </summary>
    public interface IFontFamilyResolver
    {
        /// <summary>
        /// Resolve the <see cref="IFont"/> for the specified <paramref name="xmlNode"/>.
        /// </summary>
        IFont ResolveFont(XmlNode xmlNode,
                          IXamlSemanticModel semanticModel,
                          IXamlPlatform platform,
                          IProjectFile projectFile,
                          bool searchProjectReferences = true);
    }
}
