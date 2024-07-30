using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Maui;

namespace MFractor.Editor.XAML.Completion.Services
{
    public interface IDynamicResourceCompletionService
    {
        IReadOnlyList<ICompletionSuggestion> ProvideDynamicResourceCompletions(IXamlFeatureContext featureContext, bool isShorthandMode);
    }
}