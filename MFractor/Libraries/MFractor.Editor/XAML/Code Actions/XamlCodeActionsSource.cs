using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Maui;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.CodeActions
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class XamlCodeActionsSource : ISuggestedActionsSource, IDisposable, ITelemetryIdProvider<Guid>, ISuggestedActionsSource2
    {
        [ImportingConstructor]
        public XamlCodeActionsSource(Lazy<IWorkspaceService> workspaceService,
                                     Lazy<ICodeActionEngine> codeActionEngine,
                                     Lazy<IWorkEngine> workEngine,
                                     Lazy<IXamlFeatureContextService> xamlFeatureContextService,
                                     Lazy<IAnalyticsService> analyticsService)
        {
            this.workspaceService = workspaceService;
            this.codeActionEngine = codeActionEngine;
            this.workEngine = workEngine;
            this.xamlFeatureContextService = xamlFeatureContextService;
            this.analyticsService = analyticsService;
        }

        const string codeFixCategory = "CODEFIX";
        const string refactoringCategory = "REFACTORING";
        const string anyCategory = "ANY";

        string filePath;
        ITextView textView;

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<ICodeActionEngine> codeActionEngine;
        public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IXamlFeatureContextService> xamlFeatureContextService;
        public IXamlFeatureContextService XamlFeatureContextService => xamlFeatureContextService.Value;

        readonly Lazy<IAnalyticsService> analyticsService;
        IAnalyticsService AnalyticsService => analyticsService.Value;

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        static readonly CodeActionCategory[] categoryFilters = {
            CodeActionCategory.Refactor,
            CodeActionCategory.Generate,
            CodeActionCategory.Organise,
            CodeActionCategory.Misc,
            CodeActionCategory.Find
        };

        public void Initialise(string filePath, ITextView textView)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            this.filePath = filePath;
            this.textView = textView ?? throw new ArgumentNullException(nameof(textView));
            textView.GotAggregateFocus += TextView_GotAggregateFocus;
            textView.LostAggregateFocus += TextView_LostAggregateFocus;
        }

        private void TextView_LostAggregateFocus(object sender, EventArgs e)
        {
            textView.Caret.PositionChanged -= Caret_PositionChanged;
        }

        void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            textView.Caret.PositionChanged -= Caret_PositionChanged;
            textView.Caret.PositionChanged += Caret_PositionChanged;

            if (textView.HasAggregateFocus)
            {
                SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        void Caret_PositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            textView.LostAggregateFocus -= TextView_LostAggregateFocus;
            textView.GotAggregateFocus -= TextView_GotAggregateFocus;
            textView.Caret.PositionChanged -= Caret_PositionChanged;
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories,
                                                                   SnapshotSpan range,
                                                                   CancellationToken cancellationToken)
        {
            var featureContext = XamlFeatureContextService.Retrieve(filePath);

            if (featureContext == null)
            {
                return null;
            }

            var caretOffset = textView.Caret.Position.BufferPosition.Position;
            featureContext.Syntax = XamlFeatureContextService.GetSyntaxAtLocation(featureContext.Document.AbstractSyntaxTree, caretOffset);
            var location = new InteractionLocation(caretOffset);

            var sets = new List<SuggestedActionSet>();

            var span = default(Span?);

            if (featureContext.Syntax is XmlNode xmlNode)
            {
                span = new Span(xmlNode.OpeningTagSpan.Start, xmlNode.OpeningTagSpan.Length);
            }
            else if (featureContext.Syntax is XmlAttribute xmlAttribute)
            {
                span = new Span(xmlAttribute.Span.Start, xmlAttribute.Span.Length);
            }

            if (requestedActionCategories.Contains(anyCategory))
            {
                sets.Add(CreateCodeActionSet(codeFixCategory, featureContext, location, span, CodeActionCategory.Fix));
                sets.Add(CreateCodeActionSet(refactoringCategory, featureContext, location, span, categoryFilters));
            }
            else
            {
                if (requestedActionCategories.Contains(codeFixCategory))
                {
                    sets.Add(CreateCodeActionSet(codeFixCategory, featureContext, location, span, CodeActionCategory.Fix));
                }

                if (requestedActionCategories.Contains(refactoringCategory))
                {
                    sets.Add(CreateCodeActionSet(refactoringCategory, featureContext, location, span, categoryFilters));
                }
            }

            var filteredSets = sets.Where(s => s != null && s.Actions.Any()).ToList();

            if (!filteredSets.Any())
            {
                return null;
            }

            return filteredSets;
        }

        SuggestedActionSet CreateCodeActionSet(string category,
                                               IFeatureContext featureContext,
                                               InteractionLocation location,
                                               Span? span,
                                               params CodeActionCategory[] categories)
        {
            var actions = new List<ISuggestedAction>();

            var codeActions = CodeActionEngine.RetrieveCodeActions(featureContext, location, categories);

            if (codeActions != null && codeActions.Any())
            {
                foreach (var codeAction in codeActions)
                {
                    var suggestions = codeAction.Suggest(featureContext, location);

                    foreach (var suggestion in suggestions)
                    {
                        actions.Add(new XamlCodeActionSuggestion(new CodeActionChoice(suggestion, codeAction, featureContext), CodeActionEngine, location, AnalyticsService));
                    }
                }
            }

            return new SuggestedActionSet(category, actions, null, SuggestedActionSetPriority.None, span);
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories,
                                                   SnapshotSpan range,
                                                   CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            telemetryId = Guid.Empty;
            return false;
        }

        public Task<ISuggestedActionCategorySet> GetSuggestedActionCategoriesAsync(ISuggestedActionCategorySet requestedActionCategories,
                                                                                   SnapshotSpan range,
                                                                                   CancellationToken cancellationToken)
        {
            return Task.FromResult<ISuggestedActionCategorySet>(null);
        }
    }
}
