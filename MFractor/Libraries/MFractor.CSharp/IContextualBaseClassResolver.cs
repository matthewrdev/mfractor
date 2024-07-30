using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp
{
    public delegate bool TypeFilterDelegate(INamedTypeSymbol namedType);

    public interface IContextualBaseClassResolver
    {
        INamedTypeSymbol GetSuggestedBaseClass(Project project, IReadOnlyList<string> virtualFolderPath, TypeFilterDelegate typeFilterDelegate = null, int matchHeuristic = 2);

        IReadOnlyDictionary<INamedTypeSymbol, int> GetSuggestedBaseClasses(Project project, IReadOnlyList<string> virtualFolderPath, TypeFilterDelegate typeFilterDelegate = null);
    }
}