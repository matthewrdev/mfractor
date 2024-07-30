using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion
{
    /// <summary>
    /// A service that provides completions to MFractors XAML IntelliSense that integrates with the Visual Studio editor APIs.
    /// <para/>
    /// Implementations of <see cref="IAsyncXamlCompletionService"/> are automatically added to the engine via MEF.
    /// </summary>
    [InheritedExport]
    public interface IXamlCompletionService : IAnalyticsFeature
    {
        /// <summary>
        /// Can this <see cref="IAsyncXamlCompletionService"/> provide <see cref="CompletionItem"/>'s?
        /// </summary>
        /// <param name="session"></param>
        /// <param name="featureContext"></param>
        /// <param name="xmlPath"></param>
        /// <param name="trigger"></param>
        /// <param name="triggerLocation"></param>
        /// <param name="applicableToSpan"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token);

        /// <summary>
        /// Provide <see cref="CompletionItem"/>'s that are available.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="featureContext"></param>
        /// <param name="xmlPath"></param>
        /// <param name="source"></param>
        /// <param name="trigger"></param>
        /// <param name="triggerLocation"></param>
        /// <param name="applicableToSpan"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token);
    }
}
