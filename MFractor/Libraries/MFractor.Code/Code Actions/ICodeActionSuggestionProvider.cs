using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeActions
{
    public interface ICodeActionSuggestionProvider
    {
        IReadOnlyList<ICodeActionSuggestion> CodeActionSuggestionsAtContext(Project project, string filePath, int offset, params CodeActionCategory[] filterByCategories);
    }
}
