using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    class XamlNamespace : IXamlNamespace
    {
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(Prefix))
                {
                    return "xmlns";
                }

                return "xmlns:" + Prefix;
            }
        }

        public string Prefix { get; set; }

        public bool HasValue => !string.IsNullOrEmpty(Value);

        public string Value { get; set; }

        public bool IsSchema => Schema != null;

        public IXamlSchema Schema { get; set; }

        public bool HasClrNamespaceContent => ClrNamespaceComponent != null && !string.IsNullOrEmpty(ClrNamespaceComponent.Namespace);
        public ClrNamespaceDeclaration ClrNamespaceComponent { get; set; }

        public bool HasAssemblyContent => AssemblyComponent != null && !string.IsNullOrEmpty(AssemblyComponent.AssemblyName);
        public AssemblyDeclaration AssemblyComponent { get; set; }

        public TargetPlatformDeclaration TargetPlatformComponent { get; set; }
    }
}