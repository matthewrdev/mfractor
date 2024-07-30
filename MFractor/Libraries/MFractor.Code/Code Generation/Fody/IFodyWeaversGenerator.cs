using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeGeneration.Fody
{
    public interface IFodyWeaversGenerator : ICodeGenerator
    {
        string FodyWeaversFilePath { get; set; }

        ICodeSnippet Snippet { get; set; }

        IReadOnlyList<IWorkUnit> Generate(Project project);

        IReadOnlyList<IWorkUnit> Generate(Project project, string filePath);

        string GenerateCode();

        XmlSyntaxTree GenerateSyntax();
    }
}