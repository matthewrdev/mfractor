using Microsoft.Language.Xml;

namespace MFractor.Xml
{
    public interface IXmlSyntax : IMetaDataObject
    {
        SyntaxNode RawSyntax { get; set; }

        TSyntaxNode GetRawSyntax<TSyntaxNode>() where TSyntaxNode : SyntaxNode;
    }
}