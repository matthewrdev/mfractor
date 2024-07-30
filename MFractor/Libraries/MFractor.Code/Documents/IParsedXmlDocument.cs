using System;
using MFractor.Xml;

namespace MFractor.Code.Documents
{
    public interface IParsedXmlDocument : IParsedDocument
    {
        /// <summary>
        /// Get the <see cref="AbstractSyntaxTree"/> of this document cast as <typeparamref name="TSyntaxTree"/>.
        /// </summary>
        /// <returns></returns>
        XmlSyntaxTree GetSyntaxTree();
    }
}
