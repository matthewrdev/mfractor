using MFractor.Data.Models;
using MFractor.Utilities;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// A resource declared in a C# file that has been applied to an Applications or VisualElements resource dictionary.
    /// </summary>
    public class DynamicResourceDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The name of this dynamic resource.
        /// <para/>
        /// This is the resource key that is assigned 
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// The class symbol that owns this dynamic resource definition.
        /// </summary>
        /// <value>The class symbol that owns this dynamic resource definition.</value>
        public string OwnerSymbolMetaType { get; set; }

        /// <summary>
        /// The return type of the dynamic resource, based on the right hand part of it's initialisation expression.
        /// </summary>
        /// <value>The type of the meta.</value>
        public string ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the type of the special.
        /// </summary>
        /// <value>The type of the special.</value>
        public int SpecialTypeInt { get; set; }

        
        public SpecialType SpecialType
        {
            get => (SpecialType)SpecialTypeInt;
            set => SpecialTypeInt = (int)value;
        }

        /// <summary>
        /// The starting offset for the name declaration of this dynamic resource.
        /// </summary>
        /// <value>The offset.</value>
        public int NameOffset { get; set; }

        /// <summary>
        /// The length of the C# syntax that declares the name of this dynamic resource.
        /// </summary>
        /// <value>The length of the name.</value>
        public int NameLength { get; set; }

        /// <summary>
        /// The starting offset of the expression that initiliasies this dynamic resoource.
        /// </summary>
        /// <value>The offset.</value>
        public int ExpressionOffset { get; set; }

        /// <summary>
        /// The length of expression that initialises this dynamic resource.
        /// </summary>
        /// <value>The length of the expression.</value>
        public int ExpressionLength { get; set; }

        /// <summary>
        /// The C# code of the expression that initialises this dynamic resource, encoded in Base64. 
        /// </summary>
        /// <value>The expression base64.</value>
        public string ExpressionBase64 { get; set; }

        /// <summary>
        /// The C# expression that initialise this dynamic resource.
        /// </summary>
        /// <value>The expression.</value>
        
        public string Expression
        {
            get => Base64Helper.Decode(ExpressionBase64);
            set => ExpressionBase64 = Base64Helper.Encode(value);
        }

        /// <summary>
        /// The TextSpan of the <see cref="Name"/> of this dynamic resource.
        /// </summary>
        /// <value>The name span.</value>
        
        public TextSpan NameSpan => TextSpan.FromBounds(NameOffset, NameOffset + NameLength);

        /// <summary>
        /// The TextSpan of the <see cref="Expression"/> that initialises this dynamic resource.
        /// </summary>
        /// <value>The expression span.</value>
        
        public TextSpan ExpressionSpan => TextSpan.FromBounds(ExpressionOffset, ExpressionOffset + ExpressionLength);
    }
}
