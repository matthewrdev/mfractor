using MFractor.Xml;

namespace MFractor.Maui.Semantics
{
    public interface ICodeBehindField
    {
        /// <summary>
        /// The name of this code behind field as defined by the x:Name attribute.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The parent node that this code behind field is declared for.
        /// </summary>
        XmlNode Node { get; }
    }
}