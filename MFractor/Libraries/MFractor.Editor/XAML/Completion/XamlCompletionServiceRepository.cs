using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.IOC;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlCompletionServiceRepository))]
    class XamlCompletionServiceRepository : PartRepository<IXamlCompletionService>, IXamlCompletionServiceRepository
    {
        [ImportingConstructor]
        public XamlCompletionServiceRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
        }

        readonly Logging.ILogger log = Logging.Logger.Create();

        public IReadOnlyList<IXamlCompletionService> XamlCompletionServices => Parts;

        public IEnumerable<IXamlCompletionService> GetAvailableServices(ITextView textView,
                                                                        IXamlFeatureContext context,
                                                                        XamlExpressionSyntaxNode xamlExpression,
                                                                        SnapshotPoint triggerLocation,
                                                                        SnapshotSpan applicableToSpan,
                                                                        CancellationToken token)
        {
            var services = new List<IXamlCompletionService>();

            foreach (var service in XamlCompletionServices)
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var canProvide = service.CanProvideCompletions(textView, context, xamlExpression, triggerLocation, applicableToSpan, token);

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