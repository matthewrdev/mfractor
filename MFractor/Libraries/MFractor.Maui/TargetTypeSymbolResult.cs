using System.Diagnostics;
using MFractor.Maui.Xmlns;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    [DebuggerDisplay("{TargetType} - Success: {Success}")]
    public class TargetTypeSymbolResult
    {
        public INamedTypeSymbol TargetType { get; }

        public IXamlNamespace XamlNamespace { get; }

        public bool Success { get; }

        public TargetTypeSymbolResult()
            : this(null, null)
        {
        }

        public TargetTypeSymbolResult(INamedTypeSymbol targetType, IXamlNamespace xamlNamespace)
        {
            TargetType = targetType;
            XamlNamespace = xamlNamespace;
            Success = targetType != null && xamlNamespace != null;
        }

        public readonly static TargetTypeSymbolResult Failure = new TargetTypeSymbolResult();
    }
}
