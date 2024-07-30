using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// A user defined style.
    /// </summary>
    public class StyleDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that is this style.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceId { get; set; }

        /// <summary>
        /// The fully qualified meta-type that this style targets.
        /// </summary>
        /// <value>The type of the target.</value>
        public string TargetType { get; set; }

        /// <summary>
        /// The name of this style, equavilient to the "x:Key" attribute.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// The file offset for the start of the name section of the static resource declaration.
        /// </summary>
        /// <value>The name start.</value>
        public int NameStart { get; set; }

        /// <summary>
        /// The file offset for the end of the name section of the static resource declaration.
        /// </summary>
        /// <value>The name end.</value>
        public int NameEnd { get; set; }

        /// <summary>
        /// The span of the static resource declaration name area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan NameSpan => TextSpan.FromBounds(NameStart, NameEnd);

        /// <summary>
        /// Is this style an implicit style?
        /// <para/>
        /// Implicit styles are style declared without a <see cref="Name"/>/x:Key attribute.
        /// <para/>
        /// These styles are automatically applied to all VisualElements that inherit from the <see cref="TargetType"/>.
        /// </summary>
        /// <value><c>true</c> if is implicit style; otherwise, <c>false</c>.</value>
        
        public bool IsImplicitStyle => string.IsNullOrEmpty(Name);

        /// <summary>
        /// The name of the style that this <see cref="StyleDefinition"/> derives from.
        /// </summary>
        public string BaseStyleName { get; set; }

        /// <summary>
        /// Does this style have a base style?
        /// </summary>
        
        public bool HasBaseStyle => string.IsNullOrEmpty(BaseStyleName);

        /// <summary>
        /// The file offset for the start of the static resource declaration syntax area.
        /// </summary>
        /// <value>The start offset.</value>
        public int StartOffset { get; set; }

        /// <summary>
        /// The file offset for the end of the static resource declaration syntax area.
        /// </summary>
        /// <value>The end offset.</value>
        public int EndOffset { get; set; }

        /// <summary>
        /// The span of the static resource declaration syntax area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan Span => TextSpan.FromBounds(StartOffset, EndOffset);
    }
}
