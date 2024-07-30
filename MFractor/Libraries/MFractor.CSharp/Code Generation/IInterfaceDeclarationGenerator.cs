using System;
using System.Collections.Generic;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IInterfaceDeclarationGenerator
    {
        IReadOnlyList<IWorkUnit> Generate(string interfaceName, string folderPath, Project project);

        CompilationUnitSyntax GenerateSyntax(string interfaceName, string folderPath, Project project);

        string GenerateCode(string interfaceName, string folderPath, Project project);
    }
}
