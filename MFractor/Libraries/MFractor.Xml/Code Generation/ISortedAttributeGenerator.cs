using System.Collections.Generic;
using MFractor.Code.CodeGeneration;

namespace MFractor.Xml.CodeGeneration
{
    public interface ISortedAttributeGenerator : ICodeGenerator
    {
        IEnumerable<XmlAttribute> Generate(XmlNode node);
        IEnumerable<XmlAttribute> Generate(IReadOnlyList<XmlAttribute> attributes);
    }
}
