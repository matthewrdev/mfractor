using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IMemberFieldGenerator : ICodeGenerator
    {
		bool UnderscoreOnBackingField { get; set; }

        string CreateFieldName(string candidateFieldName);

        FieldDeclarationSyntax GenerateSyntax(ITypeSymbol type, string name, string value);
    }
}
