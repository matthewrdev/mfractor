using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    public class StringResourceDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that is this style.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceKey { get; set; }

        /// <summary>
        /// The name of this color resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The literal value of this string resource.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The file offset for the start of the value section of the string declaration.
        /// </summary>
        /// <value>The name start.</value>
        public int ValueStart { get; set; }

        /// <summary>
        /// The file offset for the end of the name section of the string declaration.
        /// </summary>
        /// <value>The name end.</value>
        public int ValueEnd { get; set; }

        /// <summary>
        /// The span of the colors value area.
        /// </summary>
        /// <value>The span.</value>
        
        public TextSpan ValueSpan => TextSpan.FromBounds(ValueStart, ValueEnd);

        /// <summary>
        /// Does this string definition have a value?
        /// </summary>
        
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);
    }
}