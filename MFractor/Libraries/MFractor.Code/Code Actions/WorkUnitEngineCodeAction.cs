using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Work;
using CompilationCodeActionOperation = Microsoft.CodeAnalysis.CodeActions.CodeActionOperation;
using CompilationCodeAction = Microsoft.CodeAnalysis.CodeActions.CodeAction;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// A Roslyn <see cref="CodeAction"/> that 
    /// </summary>
    class WorkEngineCodeAction : CompilationCodeAction
    {
        readonly string title = "";
        readonly IAnalyticsService analyticsService;
        readonly IWorkEngine  workEngine;

        public override string Title => title;

        public ICodeAction CodeAction { get; }
        public IFeatureContext Context { get; }
        public InteractionLocation Location { get; set; }
        public ICodeActionSuggestion Suggestion { get; }

        public WorkEngineCodeAction(IFeatureContext context,
                                    ICodeAction codeAction,
                                    InteractionLocation location,
                                    ICodeActionSuggestion suggestion,
                                    IAnalyticsService analyticsService,
                                    IWorkEngine  workEngine)
        {
            title = suggestion.Description + "";
            Location = location;
            Suggestion = suggestion;
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
            Context = context;
            CodeAction = codeAction;
        }

        protected override Task<IEnumerable<CompilationCodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<CompilationCodeActionOperation>());
        }

        protected override Task<IEnumerable<CompilationCodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
        {
            var result = new List<CompilationCodeActionOperation>
            {
                new WorkEngineCodeActionOperation(Context, CodeAction, Location, Suggestion, workEngine, analyticsService)
            };

            return Task.FromResult<IEnumerable<CompilationCodeActionOperation>>(result);
        }
    }
}
