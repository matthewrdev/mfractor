using Microsoft.CodeAnalysis;

namespace MFractor.CSharp
{
    public class MethodParameter
    {
        public ITypeSymbol Type { get; }
        public string Name { get; }
        public string DefaultValue { get; }

        public bool IsOutParameter { get; set; } = false;
        public bool IsRefParameter { get; set; } = false;

        public MethodParameter(ITypeSymbol type,
                               string name,
                               string defaultValue)
        {
            Type = type;
            Name = name;
            DefaultValue = defaultValue;
        }
    }
}
