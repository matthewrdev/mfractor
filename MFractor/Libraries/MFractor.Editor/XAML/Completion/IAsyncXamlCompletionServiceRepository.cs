using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.IOC;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion
{
    /// <summary>
    /// Manages implementations of <see cref="IAsyncXamlCompletionService"/> and provides them to the <see cref="IAsyncXamlCompletionServiceRepository"/>.
    /// </summary>
    public interface IAsyncXamlCompletionServiceRepository : IPartRepository<IAsyncXamlCompletionService>
    {
        /// <summary>
        /// The <see cref="IAsyncXamlCompletionService"/> implementations available in the app domain.
        /// </summary>
        IReadOnlyList<IAsyncXamlCompletionService> AsyncXamlCompletionServices { get; }

        /// <summary>
        /// Get's the available <see cref="IAsyncXamlCompletionService"/> that can provide completions for the given context.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="featureContext"></param>
        /// <param name="trigger"></param>
        /// <param name="triggerLocation"></param>
        /// <param name="applicableToSpan"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<IEnumerable<IAsyncXamlCompletionService>> GetAvailableServicesAsync(ITextView textView, IXamlFeatureContext context,  XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token);
    }
}
