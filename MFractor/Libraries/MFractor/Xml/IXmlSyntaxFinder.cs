using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Xml
{
    public interface IXmlSyntaxFinder
    {
        IReadOnlyList<XmlSyntax> BuildXmlPathToOffset(XmlSyntaxTree syntaxTree, int offset, out TextSpan span);

        XmlAttribute FindAttributeAtOffset(XmlSyntaxTree document, int offset);

        XmlNode FindNodeAtOffset(XmlSyntaxTree document, int offset);

        XmlSyntax GetXmlSyntaxAtOffset(XmlSyntaxTree syntaxTree, int offset, out TextSpan span);
    }
}