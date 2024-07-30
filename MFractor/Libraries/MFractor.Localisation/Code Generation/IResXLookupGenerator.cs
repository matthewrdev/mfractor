using System;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.CodeGeneration
{
    public interface IResXLookupGenerator : ICodeGenerator
    {
        SyntaxNode Generate(INamedTypeSymbol resourceSymbol, string propertyName);

		SyntaxNode Generate(string resourceSymbol, string propertyName);
    }
}
