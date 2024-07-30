using System;
using MFractor.Code.CodeGeneration;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using MFractor.Work;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IAttributeDeclarationGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Generate(string attributeName, string folderPath, Project project);

        string GenerateCode(string attributeName, string folderPath, Project project);

        CompilationUnitSyntax GenerateSyntax(string attributeName, string folderPath, Project project);
    }
}
