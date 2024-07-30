using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Xaml
{
    /// <summary>
    /// The <see cref="IRenameXmlNodeGenerator"/> can be used to convert an XAML element from one type to another.
    /// <para/>
    /// This code generator will create a list of workUnits that converts the opening and closing tags, property setters and triggers to the new type.
    /// </summary>
    public interface IRenameXmlNodeGenerator : ICodeGenerator
    {
        /// <summary>
        /// Change the <paramref name="node"/> to use the <paramref name="newType"/> provided, updating the start and end tags, any property setters and any triggers.
        /// <para/>
        /// If <paramref name="newType"/> is not available in any of the XAML namespaces in the <paramref name="xamlDocument"/>, a <see cref="MFractor.WorkUnits.TextInputWorkUnit"/> will be raised asking the user for the name of the new XAML namespace.
        /// </summary>
        /// <returns>The change.</returns>
        /// <param name="node">Node.</param>
        /// <param name="newType">New type.</param>
        /// <param name="oldName">The old name of the XML node</param>
        /// <param name="xamlDocument">Xaml document.</param>
        IReadOnlyList<IWorkUnit> Rename(XmlNode node, INamedTypeSymbol newType, string oldName, IParsedXamlDocument xamlDocument, Project project, IXamlSemanticModel semanticModel, IXamlPlatform platform);

        /// <summary>
        /// Change the <paramref name="node"/> to use the <paramref name="newType"/> provided, updating the start and end tags, any property setters and any triggers.
        /// </summary>
        /// <returns>The change.</returns>
        /// <param name="node">Node.</param>
        /// <param name="newType">New type.</param>
        /// <param name="newTypeXamlNamespaceName">New type xaml namespace name.</param>
        /// <param name="oldName">The old name of the XML node</param>
        /// <param name="xamlDocument">Xaml document.</param>
        IReadOnlyList<IWorkUnit> Rename(XmlNode node, INamedTypeSymbol newType, string newTypeXamlNamespaceName, string oldName, IParsedXamlDocument xamlDocument, IXamlSemanticModel semanticModel, IXamlPlatform platform);

        /// <summary>
        /// Change the <paramref name="node"/> to use the <paramref name="newNodeName"/> provided, updating the start and end tags.
        /// </summary>
        /// <returns>The change.</returns>
        /// <param name="node">Node.</param>
        /// <param name="newNodeName">New type.</param>
        /// <param name="xamlDocument">Xaml document.</param>
        IReadOnlyList<IWorkUnit> Rename(XmlNode node, string newNodeName, IParsedXamlDocument xamlDocument);
    }
}
