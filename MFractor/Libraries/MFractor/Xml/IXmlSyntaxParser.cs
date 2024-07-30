using System.Collections.Generic;
using System.IO;
using MFractor.Text;

namespace MFractor.Xml
{
    /// <summary>
    /// 
    /// </summary>
    public interface IXmlSyntaxParser
    {
        XmlSyntaxTree ParseFile(FileInfo file);
        XmlSyntaxTree ParseFile(string filePath);
        XmlSyntaxTree ParseStream(Stream stream);

        XmlSyntaxTree ParseText(string content);
        XmlSyntaxTree ParseText(ITextProvider textProvider);

        XmlSyntaxTree ParseTextIncremental(string content, XmlSyntaxTree previous, IEnumerable<ITextReplacement> replacements);
    }
}