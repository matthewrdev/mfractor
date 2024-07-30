using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using CompilationCodeAction = Microsoft.CodeAnalysis.CodeActions.CodeAction;

namespace MFractor.Code.CodeActions
{
    class DocumentChangeAction : CompilationCodeAction
    {
        readonly Func<CancellationToken, Task<Document>> createChangedDocument;

        public IAnalyticsFeature AnalyticsFeature { get; }

        public IAnalyticsService AnalyticsService { get; }

        public DocumentChangeAction(string title, 
                                    Func<CancellationToken, Task<Document>> createChangedDocument,
                                    IAnalyticsFeature analyticsFeature,
                                    IAnalyticsService analyticsService)
        {
            AnalyticsFeature = analyticsFeature;
            AnalyticsService = analyticsService;
            Title = title;
            this.createChangedDocument = createChangedDocument;
        }

        protected override Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
        {
            return base.ComputePreviewOperationsAsync(cancellationToken);
        }

        public override string Title { get; }

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            return createChangedDocument(cancellationToken);
        }

        protected override Task<Document> PostProcessChangesAsync(Document document, CancellationToken cancellationToken)
        {
            return base.PostProcessChangesAsync(document, cancellationToken);
        }

        protected override async Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
        {
            var operations = (await base.ComputeOperationsAsync(cancellationToken)).ToList();

            if (AnalyticsFeature != null)
            {
                operations.Add(new CodeActionAnalyticsActionOperation(AnalyticsFeature, AnalyticsService));
            }

            return operations;
        }
    }
}
