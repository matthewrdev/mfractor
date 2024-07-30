using MFractor.Code;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// An <see cref="IFeatureContext"/> for XAML flavoured documents.
    /// </summary>
    public interface IXamlFeatureContext : IFeatureContext
    {
        /// <summary>
        /// The compilation 
        /// </summary>
        Compilation Compilation { get; }

        /// <summary>
        /// The semantic model for this XAML context that can be used to resolve symbols for syntax.
        /// </summary>
        IXamlSemanticModel XamlSemanticModel { get; }

        /// <summary>
        /// The XAML namespaces defined by this XAML file.
        /// </summary>
        /// <value>The namespaces.</value>
        IXamlNamespaceCollection Namespaces { get; }

        /// <summary>
        /// The XMLNS definitions available in the app domain.
        /// </summary>
        IXmlnsDefinitionCollection XmlnsDefinitions { get; }

        /// <summary>
        /// The XAML Document for this feature context.
        /// </summary>
        IParsedXamlDocument XamlDocument { get; }

        /// <summary>
        /// The XAML platorm that this context targets.
        /// <para/>
        /// See <see cref="MFractor.Maui.XamlPlatform"/> for a list of supported or planned platforms.
        /// </summary>
        IXamlPlatform Platform {get;}

        /// <summary>
        /// The XML syntax tree for the <see cref="XamlDocument"/>.
        /// </summary>
        XmlSyntaxTree SyntaxTree { get; }
    }
}