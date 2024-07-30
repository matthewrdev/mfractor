using System.Collections.Generic;
using System.IO;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.CodeGeneration
{
    public interface IResXFileGenerator : ICodeGenerator
    {
        ICodeSnippet ResXTemplate { get; set; }

        ICodeSnippet ResXDesignerTemplate { get; set; }

        IReadOnlyList<IWorkUnit> GenerateResourceFile(Project project, string resourceFilePath, bool includeDesignerFile);

        IReadOnlyList<IWorkUnit> GenerateResourceFile(Project project, string resourceFilePath, IEnumerable<ILocalisationValue> values, bool includeDesignerFile);
    }
}
