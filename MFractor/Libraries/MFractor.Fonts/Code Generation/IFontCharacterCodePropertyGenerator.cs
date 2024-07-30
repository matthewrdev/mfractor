using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Fonts.CodeGeneration
{
    public interface IFontCharacterCodePropertyGenerator : ICodeGenerator
    {
        ICodeSnippet Template { get; }

        string GenerateCode(string name, string code);

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string name, string code);
    }
}
