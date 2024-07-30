using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.ResourceDictionaries
{
    public interface IImportResourceDictionaryGenerator : ICodeGenerator
    {
        IReadOnlyList<IWorkUnit> ImportResourceDictionary(ParsedXamlDocument parsedXamlDocument, INamedTypeSymbol resourceDictionarySymbol);
        IReadOnlyList<IWorkUnit> ImportResourceDictionary(ParsedXamlDocument parsedXamlDocument, string resourceDictionaryType);
    }
}
