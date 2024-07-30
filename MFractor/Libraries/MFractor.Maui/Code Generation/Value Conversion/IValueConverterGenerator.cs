using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.ValueConversion
{
    public interface IValueConverterGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        IValueConversionSettings ValueConversionSettings { get; set; }

        bool CreateMissingValueConversionAttribute { get; set; }

        string GetConverterTypeName(ValueConverterGenerationOptions options);
        string GetConverterTypeName(string name, ProjectIdentifier project, string folderPath, string @namespace);

        CompilationUnitSyntax GenerateSyntax(ValueConverterGenerationOptions generationOptions);
        CompilationUnitSyntax GenerateSyntax(string name, ProjectIdentifier project, IXamlPlatform platform, string @namespace, string inputType = "object", string outputType = "object", string parameterType = null);

        string GenerateCode(ValueConverterGenerationOptions generationOptions);
        string GenerateCode(string name, ProjectIdentifier project, IXamlPlatform platform, string @namespace, string inputType = "object", string outputType = "object", string parameterType = null);

        IReadOnlyList<IWorkUnit> Generate(ValueConverterGenerationOptions generationOptions);
        IReadOnlyList<IWorkUnit> Generate(string name, ProjectIdentifier project, IXamlPlatform platform, string folderPath, string @namespace, string inputType = "object", string outputType = "object", string parameterType = null, IProjectFile xamlEntryFile = null);
    }
}
