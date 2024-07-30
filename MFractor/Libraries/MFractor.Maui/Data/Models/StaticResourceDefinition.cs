using MFractor.Data.Models;
using MFractor.Utilities;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;
using MFractor.Maui.XamlPlatforms;


namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// A resource declared in a XAML file inside a resource dictionary that has an x:Key attribute appleid.
    /// </summary>
    public class StaticResourceDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The x:Key value of this static resource.
        /// <para/>
        /// This value may be null when the user has defined an implicit style.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// The name of the symbol. For example, `OnPlatform`.
        /// </summary>
        /// <value>The symbol.</value>
        public string SymbolName { get; set; }

        /// <summary>
        /// The namespace or schema of the symbol.
        /// </summary>
        /// <value>The symbol namespace.</value>
        public string SymbolNamespace { get; set; }

        /// <summary>
        /// The assembly that owns the symbol where that the static resource is.
        /// </summary>
        /// <value>The symbol assembly.</value>
        public string SymbolAssembly { get; set; }

        /// <summary>
        /// The fully qualified meta-data name that is the return type of the static resource.
        /// <para/>
        /// For example, for OnIdiom or OnPlatform values,
        /// </summary>
        /// <value>The type of the return.</value>
        public string ReturnType { get; set; }

        /// <summary>
        /// The fully qualified meta-data name that this static resource targets.
        /// <para/>
        /// For example, for Style's or Trigger's.
        /// </summary>
        /// <value>The type of the target.</value>
        public string TargetType { get; set; }

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

        /// <summary>
        /// The fully qualified symbol name that can be used to locate the <see cref="Microsoft.CodeAnalysis.INamedTypeSymbol"/> for this static resource definition.
        /// </summary>
        /// <value>The name of the meta data.</value>
        
        public string SymbolMetaType
        {
            get
            {
                if (string.IsNullOrEmpty(SymbolName))
                {
                    return "";
                }

                if (string.IsNullOrEmpty(SymbolNamespace))
                {
                    return "";
                }

                return SymbolNamespace + "." + SymbolName;
            }
        }

        /// <summary>
        /// If this resource is omitted an x:Key value.
        /// <para/>
        /// For static resources that are a patform Style, implicit resources become styles that are automatically applied to child elements that match the <see cref="TargetType"/> in the XAML document.
        /// </summary>
        
        public bool IsImplicitResource => string.IsNullOrEmpty(Name);

        /// <summary>
        /// If this resource explicitly specifies an x:Key and must be used using a static resource expression.
        /// </summary>
        public bool IsExplicitResource => !IsImplicitResource;

        /// <summary>
        /// If this <see cref="SymbolMetaType"/> derives from <see cref="IXamlPlatform.Style"/>
        /// </summary>
        public bool IsStyleMetaType { get; set; }

        /// <summary>
        /// The base 64 encoded <see cref="PreviewString"/>.
        /// </summary>
        public string PreviewStringBase64 { get; set; }

        /// <summary>
        /// The xml content that may be used to preview this XAML node.
        /// </summary>
        
        public string PreviewString
        {
            get => Base64Helper.Decode(PreviewStringBase64);
            set => PreviewStringBase64 = Base64Helper.Encode(value);
        }

        public override string ToString()
        {
            return Name + " " + SymbolMetaType; 
        }
    }
}
