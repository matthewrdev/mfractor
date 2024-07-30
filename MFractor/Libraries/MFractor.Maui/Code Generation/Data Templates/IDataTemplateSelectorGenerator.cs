using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.DataTemplates
{
    public interface IDataTemplateSelectorGenerator : ICodeGenerator
    {
        ICodeSnippet Template { get; set; }

        string Namespace { get; set; }

        string Folder { get; set; }

        string GenerateCode(Project project, string dataTemplateSelectorName);

        string GenerateCode(string namespaceName, string dataTemplateSelectorName);

        CompilationUnitSyntax GenerateSyntax(Project project, string dataTemplateSelectorName);

        CompilationUnitSyntax GenerateSyntax(string namespaceName, string dataTemplateSelectorName);

        IReadOnlyList<IWorkUnit> Generate(Project project, string dataTemplateSelectorName);

        IReadOnlyList<IWorkUnit> Generate(Project project, string namespaceName, string filePath, string dataTemplateSelectorName);

        string GetNamespaceFor(Project project, string namespacePath);
    }
}
