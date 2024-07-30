using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAsyncXamlCompletionServiceRepository))]
    class AsyncXamlCompletionServiceRepository : PartRepository<IAsyncXamlCompletionService>, IAsyncXamlCompletionServiceRepository
    {
        [ImportingConstructor]
        public AsyncXamlCompletionServiceRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        readonly Logging.ILogger log = Logging.Logger.Create();

        public IReadOnlyList<IAsyncXamlCompletionService> AsyncXamlCompletionServices => Parts;

        public async Task<IEnumerable<IAsyncXamlCompletionService>> GetAvailableServicesAsync(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var services = new List<IAsyncXamlCompletionService>();

            foreach (var service in AsyncXamlCompletionServices)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var canProvide = await service.CanProvideCompletionsAsync(textView, context, xamlExpression, triggerLocation, applicableToSpan, token);

                    if (canProvide)
                    {
                        services.Add(service);
                    }
                }
                catch (OperationCanceledException oex)
                {
                    throw oex;
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            return services;
        }
    }
}