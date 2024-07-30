using System;
namespace MFractor.Xml
{
    /// <summary>
    /// The event arguments that provide the updated
    /// </summary>
    public class XmlSyntaxTreeEventArgs : EventArgs
    {
        public XmlSyntaxTreeEventArgs(XmlSyntaxTree xmlSyntaxTree, string filePath)
        {
            XmlSyntaxTree = xmlSyntaxTree;
            FilePath = filePath;
        }

        public XmlSyntaxTree XmlSyntaxTree { get; }

        public string FilePath { get; }
    }
}
