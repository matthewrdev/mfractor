using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Maui;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    public interface IImageAssetCompletionService
    {
        IReadOnlyList<ICompletionSuggestion> ProvideImageActionCompletions(ITextView textView, IXamlFeatureContext context);
        IReadOnlyList<ICompletionSuggestion> ProvideImageAssetCompletions(IXamlFeatureContext context);
    }
}