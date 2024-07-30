using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Maui;
using Microsoft.CodeAnalysis;

namespace MFractor.Editor.XAML.Completion.Services
{
    public interface IStaticResourceCompletionService
    {
        Task<IReadOnlyList<ICompletionSuggestion>> ProvideStaticResourceCompletions(IXamlFeatureContext featureContext, IPropertySymbol property, bool isShorthand);
    }
}