using System.Collections.Generic;
using MFractor.Maui;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    /// <summary>
    /// Provides completion for data binding expressions.
    /// <para/>
    /// Use the <see cref="IBindingContextResolver"/> to resolve the binding context for a XAML element.
    /// </summary>
    public interface IDataBindingCompletionService
    {
        IReadOnlyList<ICompletionSuggestion> ProvideBindingCodeActionCompletions(IXamlFeatureContext context, ITextBuffer textBuffer, IPropertySymbol property, INamedTypeSymbol bindingContext);

        IReadOnlyList<ICompletionSuggestion> ProvideBindingContextCompletions(IXamlFeatureContext context, INamedTypeSymbol bindingContext, bool isShorthandMode);
    }
}