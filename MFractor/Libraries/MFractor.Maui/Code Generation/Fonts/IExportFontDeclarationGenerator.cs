using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.Fonts
{
    public interface IExportFontDeclarationGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        string GenerateCode(string fontAssetFileName);
        SyntaxNode GenerateSyntax(string fontAssetFileName);
        IReadOnlyList<IWorkUnit> Generate(Project project, string fontAssetFileName);
    }
}