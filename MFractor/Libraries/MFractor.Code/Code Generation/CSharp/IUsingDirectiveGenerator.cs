using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Code.CodeGeneration.CSharp
{
    public interface IUsingDirectiveGenerator : ICodeGenerator
    {
        UsingDirectiveSyntax GenerateSyntax(INamespaceSymbol namespaceSymbol);
        UsingDirectiveSyntax GenerateSyntax(string namespaceName);
    }
}
