using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using MFractor.Work;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IInterfaceImplementationGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> Generate(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project);

        CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project);

        string GenerateCode(CompilationWorkspace workspace, INamedTypeSymbol interfaceType, string folderPath, string className, Project project);

        IReadOnlyList<IWorkUnit> Generate(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project);

        CompilationUnitSyntax GenerateSyntax(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project);

        string GenerateCode(CompilationWorkspace workspace, string interfaceType, string folderPath, string className, Project project);
    }
}
