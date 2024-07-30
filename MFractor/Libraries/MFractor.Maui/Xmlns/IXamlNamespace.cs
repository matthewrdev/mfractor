using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Xmlns
{
    public interface IXamlNamespace
    {
        string FullName { get; }

        string Prefix { get; }

        string Value { get; }

        IXamlSchema Schema { get; }

        ClrNamespaceDeclaration ClrNamespaceComponent { get; }

        AssemblyDeclaration AssemblyComponent { get; }

        TargetPlatformDeclaration TargetPlatformComponent { get; }

        bool HasClrNamespaceContent { get; }

        bool HasAssemblyContent { get; }

        bool HasValue { get; }

        bool IsSchema { get; }
    }
}