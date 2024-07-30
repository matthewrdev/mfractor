using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IConstructorGenerator : ICodeGenerator
	{
        bool ArgumentFirstLetterToLowerCase { get; set; }
        
        ConstructorDeclarationSyntax GenerateSyntax(List<Tuple<ITypeSymbol, string>> parameters, string className);
    }
}
