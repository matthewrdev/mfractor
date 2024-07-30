using MFractor.Code.Documents;
using MFractor.Workspace;
using MFractor.Xml;

namespace MFractor.Code.Documents
{
    /// <summary>
    /// A parsed xml document with an attached <see cref="XmlSyntaxTree"/>.
    /// </summary>
    public class ParsedXmlDocument : ParsedDocument<XmlSyntaxTree>, IParsedXmlDocument
    {
        public ParsedXmlDocument(string filePath, 
                                 XmlSyntaxTree syntaxTree,
                                 IProjectFile projectFile)
            : base(filePath, syntaxTree, projectFile)
        {
        }
    }
}
