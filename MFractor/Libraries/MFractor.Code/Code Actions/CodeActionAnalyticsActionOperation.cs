using System;
using System.Threading;
using MFractor.Analytics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A <see cref="CodeActionOperation"/> implementation that sends an analytics event for <see cref="AnalyticsFeature"/> when the code action is applied.
    /// </summary>
    class CodeActionAnalyticsActionOperation : CodeActionOperation
    {
        readonly IAnalyticsService analyticsService;

        public IAnalyticsFeature AnalyticsFeature { get; }

        public CodeActionAnalyticsActionOperation(IAnalyticsFeature analyticsFeature, IAnalyticsService analyticsService)
        {
            AnalyticsFeature = analyticsFeature ?? throw new ArgumentNullException(nameof(analyticsFeature));
            this.analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
        }

        public override void Apply(CompilationWorkspace workspace, CancellationToken cancellationToken)
        {
            analyticsService.Track(AnalyticsFeature.AnalyticsEvent);
        }
    }
}
