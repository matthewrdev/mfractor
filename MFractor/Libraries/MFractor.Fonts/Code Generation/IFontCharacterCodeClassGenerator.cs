using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Fonts.CodeGeneration
{
    public interface IFontCharacterCodeClassGenerator : ICodeGenerator
    {
        string GenerateCode(IReadOnlyList<IFont> fonts, string className, string @namespace);

        CompilationUnitSyntax GenerateSyntax(IReadOnlyList<IFont> fonts, string className, string @namespace);

        IReadOnlyList<IWorkUnit> Generate(IReadOnlyList<IFont> fonts, string className, string folderPath, Project targetProject);

        string GenerateCode(IFont font, string className, string @namespace);

        CompilationUnitSyntax GenerateSyntax(IFont font, string className, string @namespace);

        IReadOnlyList<IWorkUnit> Generate(IFont font, string className, string folderPath, Project targetProject);
    }
}
