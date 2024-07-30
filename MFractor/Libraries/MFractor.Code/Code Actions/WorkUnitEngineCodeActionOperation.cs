using System.Linq;
using System.Threading;
using MFractor.Analytics;
using MFractor.Licensing;
using MFractor.Work;
using Microsoft.CodeAnalysis.CodeActions;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A <see cref="CodeActionOperation"/> that executes the <see cref="CodeAction"/> and then runs the <see cref="IWorkUnit"/>'s into the workUnit engine.
    /// </summary>
    class WorkEngineCodeActionOperation : CodeActionOperation
    {
        public IFeatureContext Context { get; }
        public ICodeAction CodeAction { get; }
        public InteractionLocation Location { get; }
        public ICodeActionSuggestion Suggestion { get; }

        readonly IWorkEngine  workEngine;
        readonly IAnalyticsService analyticsService;

        public WorkEngineCodeActionOperation(IFeatureContext context,
                                             ICodeAction codeAction,
                                             InteractionLocation location,
                                             ICodeActionSuggestion suggestion,
                                             IWorkEngine  workEngine,
                                             IAnalyticsService analyticsService)
        {
            this.Context = context ?? throw new System.ArgumentNullException(nameof(context));
            this.CodeAction = codeAction ?? throw new System.ArgumentNullException(nameof(codeAction));
            this.Location = location ?? throw new System.ArgumentNullException(nameof(location));
            this.Suggestion = suggestion ?? throw new System.ArgumentNullException(nameof(suggestion));
            this.workEngine = workEngine ?? throw new System.ArgumentNullException(nameof(workEngine));
            this.analyticsService = analyticsService ?? throw new System.ArgumentNullException(nameof(analyticsService));
        }

        public override void Apply(CompilationWorkspace workspace, CancellationToken cancellationToken)
        {
            var result = CodeAction.Execute(Context, Suggestion, Location).ToList();

            workEngine.ApplyAsync(result);

            analyticsService.Track(CodeAction.AnalyticsEvent);
        }
    }
}
