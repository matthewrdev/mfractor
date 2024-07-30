using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IBaseConstructorGenerator : ICodeGenerator
	{
        ConstructorDeclarationSyntax GenerateSyntax(IMethodSymbol constructor, string classType);
    }
}
