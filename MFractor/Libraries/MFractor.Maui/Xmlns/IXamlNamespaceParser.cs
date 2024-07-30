using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MFractor.Xml;

namespace MFractor.Maui.Xmlns
{
    /// <summary>
    /// The <see cref="IXamlNamespaceParser"/> is used to create new <see cref="IXamlNamespace"/>'s.
    /// </summary>
    public interface IXamlNamespaceParser
    {
        IXamlNamespace Parse(string name, string value);

        IXamlNamespaceCollection ParseNamespaces(XmlSyntaxTree syntaxTree);
    }
}