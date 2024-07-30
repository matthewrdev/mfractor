using System;

namespace MFractor.Xml
{
    /// <summary>
    /// The <see cref="IXmlSyntaxTreeService"/> is used ot 
    /// </summary>
    public interface IXmlSyntaxTreeService
    {
        /// <summary>
        /// Gets the cached <see cref="XmlSyntaxTree"/> for the given <paramref name="filePath"/>
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        XmlSyntaxTree GetSyntaxTree(string filePath);

        /// <summary>
        /// An event that is triggered when an <see cref="XmlSyntaxTree"/> is updated for a file path.
        /// </summary>
        event EventHandler<XmlSyntaxTreeEventArgs> SyntaxTreeUpdated;

        /// <summary>
        /// An event that is triggered when the <see cref="XmlSyntaxTree"/> is removed from the file path.
        /// </summary>
        event EventHandler<XmlSyntaxTreeEventArgs> SyntaxTreeRemoved;
    }
}
