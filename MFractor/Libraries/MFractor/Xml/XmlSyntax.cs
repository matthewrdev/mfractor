using Microsoft.Language.Xml;

namespace MFractor.Xml
{
    /// <summary>
    /// A piece of xml syntax that exists within an <see cref="XmlSyntaxTree"/>.
    /// </summary>
    public abstract class XmlSyntax : MetaDataObject, IXmlSyntax
    {
        /// <summary>
        /// The original <see cref="Microsoft.Language.Xml.SyntaxNode"/> that represents this syntax element.
        /// </summary>
        public SyntaxNode RawSyntax { get; set; }

        /// <summary>
        /// Gets the original <see cref="Microsoft.Language.Xml.SyntaxNode"/>, cast as <typeparamref name="TSyntaxNode"/>, that represents this syntax element.
        /// </summary>
        /// <typeparam name="TSyntaxNode"></typeparam>
        /// <returns></returns>
        public TSyntaxNode GetRawSyntax<TSyntaxNode>() where TSyntaxNode : SyntaxNode
        {
            return RawSyntax as TSyntaxNode;
        }
    }
}

