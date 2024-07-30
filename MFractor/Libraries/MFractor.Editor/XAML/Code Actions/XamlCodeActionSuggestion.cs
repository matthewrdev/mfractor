using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Code.CodeActions;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.CodeActions
{
    class XamlCodeActionSuggestion : ISuggestedAction, IDisposable, ITelemetryIdProvider<Guid>
    {
        readonly IAnalyticsService analyticsService;
        readonly ICodeActionChoice codeActionChoice;
        readonly ICodeActionEngine codeActionEngine;
        readonly InteractionLocation interactionLocation;

        public XamlCodeActionSuggestion(ICodeActionChoice codeActionChoice,
                                        ICodeActionEngine codeActionEngine,
                                        InteractionLocation interactionLocation,
                                        IAnalyticsService analyticsService)
        {
            this.codeActionChoice = codeActionChoice;
            this.codeActionEngine = codeActionEngine;
            this.interactionLocation = interactionLocation;
            this.analyticsService = analyticsService;
        }

        public bool HasActionSets => false;

        public string DisplayText => codeActionChoice.Suggestion.Description;

        public ImageMoniker IconMoniker => default;

        public string IconAutomationText => string.Empty;

        public string InputGestureText => string.Empty;

        public bool HasPreview => false;

        public void Dispose()
        {
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult< IEnumerable<SuggestedActionSet>>(null);
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(null);
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            codeActionEngine.Execute(codeActionChoice, interactionLocation).ConfigureAwait(false);

            analyticsService.Track(codeActionChoice.CodeAction.AnalyticsEvent);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
