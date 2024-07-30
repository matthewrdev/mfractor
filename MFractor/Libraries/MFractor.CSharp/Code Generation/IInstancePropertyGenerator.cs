using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IInstancePropertyGenerator : ICodeGenerator
	{
        ICodeSnippet Snippet { get; set; }

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(ITypeSymbol propertyType, 
                                                            Accessibility accesibility, 
                                                            string propertyName, 
                                                            string propertyValue);

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string propertyType,
                                                            Accessibility accesibility,
                                                            string propertyName,
                                                            string propertyValue);

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(MemberDeclaration propertyDeclaration);
	}
}
