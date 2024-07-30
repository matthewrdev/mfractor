using System;
using System.ComponentModel;

namespace MFractor.Code.CodeGeneration
{
    /// <summary>
    /// When a SyntaxNode or XmlObject is inserted into a document automatically, this enum specifies where it should be placed.
    /// </summary>
    public enum InsertionLocation
    {
        /// <summary>
        /// The syntax should be inserted at the start of the class.
        /// </summary>
        [Description("syntax code should be inserted at the start of the host or before the anchor.")]
        Start,

        /// <summary>
        /// The syntax should be inserted at the end of the host or after the anchor.
        /// </summary>
        [Description("The syntax should be inserted at the end of the host or after the anchor.")]
        End,

        /// <summary>
        /// The syntax should be inserted in the IDEs default behaviour relative to the host or anchor.
        /// </summary>
        [Description("The syntax should be inserted in the IDEs default behaviour relative to the host or anchor.")]
        Default,
    }
}
