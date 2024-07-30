using System;
using MFractor.Code.CodeGeneration;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using MFractor.Work;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IEventArgsDeclarationGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Generate(string eventArgsName, string folderPath, Project project);

        string GenerateCode(string eventArgsName, string folderPath, Project project);

        CompilationUnitSyntax GenerateSyntax(string eventArgsName, string folderPath, Project project);
    }
}
