using System;
using System.Collections.Generic;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration.ClassFromClipboard
{
    public interface IClassFromStringContentGenerator : ICodeGenerator
    {
        //bool CanCreateClassFromContent(FileInfo fileInfo);

        bool CanCreateClassFromContent(string content);

        bool CanCreateClassFromContent(CompilationUnitSyntax ast);

        void GetTypeAndNamespaceSyntax(string content, out TypeDeclarationSyntax type, out NamespaceDeclarationSyntax @namespace);

        void GetTypeAndNamespaceSyntax(CompilationUnitSyntax ast, out TypeDeclarationSyntax type, out NamespaceDeclarationSyntax @namespace);

        CompilationUnitSyntax GenerateSyntax(Project project, string className, string @namespace, string content);

        CompilationUnitSyntax GenerateSyntax(Project project, string className, string @namespace, CompilationUnitSyntax ast);

        string GenerateCode(Project project, string className, string @namespace, string content);

        string GenerateCode(Project project, string className, string @namespace, CompilationUnitSyntax ast);

        IReadOnlyList<IWorkUnit> Generate(Project project, string className, string folderPath, string @namespace, string content);

        IReadOnlyList<IWorkUnit> Generate(Project project, string className, string folderPath, string @namespace, CompilationUnitSyntax ast);

        IReadOnlyList<IWorkUnit> Generate(CreateClassFromClipboardOptions options, string content);
        
        IReadOnlyList<IWorkUnit> Generate(CreateClassFromClipboardOptions options, CompilationUnitSyntax ast);
    }
}