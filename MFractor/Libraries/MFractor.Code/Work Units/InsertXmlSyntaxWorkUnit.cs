using MFractor.Code.CodeGeneration;
using MFractor.Work.WorkUnits;
using MFractor.Xml;

namespace MFractor.Code.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> inserts an <see cref="XmlSyntax"/> element into the <see cref="HostSyntax"/>, using the <see cref="AnchorSyntax"/> and <see cref="InsertionLocation"/> to control the insertion position.
    /// </summary>
    public class InsertXmlSyntaxWorkUnit : XmlWorkUnit
    {
        public InsertXmlSyntaxWorkUnit()
        {
        }

        public InsertXmlSyntaxWorkUnit(XmlSyntax syntax, XmlNode hostSyntax, string filePath)
        {
            FilePath = filePath;
            Syntax = syntax;
            HostSyntax = hostSyntax;
        }

        /// <summary>
        /// The parent XML node that the <see cref="Syntax"/> will be inserted into.
        /// </summary>
        public XmlNode HostSyntax { get; set; }

        /// <summary>
        /// The xml syntax to insert.
        /// </summary>
        public XmlSyntax Syntax { get; set; }

        /// <summary>
        /// The syntax node that the <see cref="Syntax"/> is inserted against.
        /// <para/>
        /// Use <see cref="InsertionLocation"/> to place the syntax before or after the anchor.
        /// </summary>
        public XmlSyntax AnchorSyntax { get; set; }

        /// <summary>
        /// The location to place the <see cref="Syntax"/> relative to the <see cref="AnchorSyntax"/>.
        /// <para/>
        /// If no <see cref="AnchorSyntax"/> is specified, the <see cref="Syntax"/> are placed at the start or end of the <see cref="HostSyntax"/>.
        /// </summary>
        public InsertionLocation InsertionLocation
        {
            get; set;
        } = InsertionLocation.Start;
    }
}
