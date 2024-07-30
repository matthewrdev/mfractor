using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    public interface IValueConversionAttributeImplementationGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        IValueConversionSettings ValueConversionSettings { get; set; }

        CompilationUnitSyntax GenerateSyntax(ProjectIdentifier project);

        string GenerateCode(ProjectIdentifier project);

        IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier project);

        IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier project, string filePath);
    }
}
