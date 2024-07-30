using System;
using System.Linq;

namespace MFractor.Maui.XamlPlatforms
{
    public class TypeDefinition : ITypeDefinition
    {
        public TypeDefinition(string metaType)
        {
            if (string.IsNullOrEmpty(metaType))
            {
                throw new ArgumentException($"'{nameof(metaType)}' cannot be null or empty.", nameof(metaType));
            }

            var components = metaType.Split('.');
            MetaType = metaType;
            Name = components.Last();
            Namespace = components.Length > 1 ? string.Join(".", components.Take(components.Length - 1)) : "";
            MarkupExpressionName = Name.EndsWith("Extension") ? Name.Substring(0, Name.Length - "Extension".Length) : string.Empty;
            IsGeneric = Name.Contains("<");
            NonGenericName = Name.Split('<').First();
        }

        public string MetaType { get; }

        public string Name { get; }

        public string NonGenericName { get; }

        public string MarkupExpressionName { get; }

        public string Namespace { get; }

        public bool IsGeneric { get; }

        public override string ToString()
        {
            return MetaType;
        }

        public static readonly ITypeDefinition Undefined = new TypeDefinition("UNDEFINED");
    }
}
