using MFractor.Maui.Xmlns;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    /// <summary>
    /// The <see cref="ITargetTypeSymbolResolver"/> resolves 
    /// </summary>
    public interface ITargetTypeSymbolResolver
    {
        TargetTypeSymbolResult GetTargetTypeSymbolForNode(XmlNode node,
                                                          Project project,
                                                          IXamlNamespaceCollection namespaces,
                                                          IXmlnsDefinitionCollection xmlnsDefinitions,
                                                          string attributeName,
                                                          bool searchParents);
    }
}
