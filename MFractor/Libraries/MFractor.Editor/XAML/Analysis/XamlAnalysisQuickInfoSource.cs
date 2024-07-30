using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Code.Analysis;
using MFractor.Analytics;
using MFractor.Documentation;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using MFractor.Code;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Language.StandardClassification;
using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Editor.XAML.Analysis
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class XamlAnalysisQuickInfoSource : IAsyncQuickInfoSource
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IAnalysisResultStore analysisResultStore;
        readonly IAnalyticsService analyticsService;
        readonly ICodeAnalyserRepository codeAnalysers;
        readonly IFeatureContextFactoryRepository featureContextFactories;
        readonly ICodeActionEngine codeActionEngine;
        readonly IWorkspaceService workspaceService;
        readonly IWorkEngine workEngine;

        string filePath;

        [ImportingConstructor]
        public XamlAnalysisQuickInfoSource(IAnalysisResultStore analysisResultStore,
                                           IAnalyticsService analyticsService,
                                           ICodeAnalyserRepository codeAnalysers,
                                           IFeatureContextFactoryRepository featureContextFactories,
                                           ICodeActionEngine codeActionEngine,
                                           IWorkspaceService workspaceService,
                                            IWorkEngine workEngine)
        {
            this.analysisResultStore = analysisResultStore;
            this.analyticsService = analyticsService;
            this.codeAnalysers = codeAnalysers;
            this.featureContextFactories = featureContextFactories;
            this.codeActionEngine = codeActionEngine;
            this.workspaceService = workspaceService;
            this.workEngine = workEngine;
        }

        public void SetFile(string filePath)
        {
            this.filePath = filePath;
        }

        // This is called on a background thread.
        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                var textBuffer = session.TextView.TextBuffer;
                var triggerPoint = session.GetTriggerPoint(textBuffer.CurrentSnapshot);

                var issues = analysisResultStore.Retrieve(filePath).Where(i => !i.IsSilent);

                if (!issues.Any() || !triggerPoint.HasValue)
                {
                    return null;
                }

                var position = triggerPoint.Value.Position;

                var issue = issues.FirstOrDefault(i => i.Span.IntersectsWith(triggerPoint.Value.Position));
                if (issue == null)
                {
                    return null;
                }

                var project = TextBufferHelper.GetCompilationProject(textBuffer);
                if (project == null)
                {
                    return null;
                }

                var factory = featureContextFactories.GetInterestedFeatureContextFactory(project, filePath);
                var context = factory.CreateFeatureContext(project, filePath, position);

                if (context == null)
                {
                    return null;
                }

                context.Syntax = factory.GetSyntaxAtLocation(context.Document.AbstractSyntaxTree, position);
                (context.Syntax as XmlSyntax)?.Add(MFractor.MetaDataKeys.Analysis.Issues, issue.AsList()); // HACK

                var textRuns = new List<ClassifiedTextRun>()
                {
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, "MFractor: " + issue.Message)
                };

                var fixes = BuildFixes(issue, context, new InteractionLocation(position));
                if (fixes.Any())
                {
                    textRuns.AddRange(fixes);
                }

                var textElement = new ClassifiedTextElement(textRuns);
                var span = textBuffer.CurrentSnapshot.CreateTrackingSpan(Span.FromBounds(issue.Span.Start, issue.Span.End), SpanTrackingMode.EdgeInclusive);
                Track(issue);

                return new QuickInfoItem(span, textElement);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return null;
        }

        private List<ClassifiedTextRun> BuildFixes(ICodeIssue issue, IFeatureContext context, InteractionLocation location)
        {
            var fixes = new List<ClassifiedTextRun>();
            var actions = codeActionEngine.RetrieveCodeActions(context, location, CodeActionCategory.Fix);

            void codeFixClicked(CodeIssueFixSelectedEventArgs args)
            {
                try
                {
                    var action = actions.FirstOrDefault(a => a.Identifier == args.CodeActionSuggestion.CodeActionIdentifier);
                    if (action != null)
                    {
                        if (!string.IsNullOrEmpty(action.AnalyticsEvent))
                        {
                            analyticsService.Track(action.AnalyticsEvent + " (Tooltip Context)");
                        }

                        var work = action.Execute(context, args.CodeActionSuggestion, location);

                        workEngine.ApplyAsync(work);

                        var id = MFractor.Workspace.Utilities.ProjectIdentifierExtensions.GetIdentifier(context.Project).Guid;
                        (workspaceService as WorkspaceService)?.NotifyFileChanged(id, context.Document.FilePath);
                    }
                    else
                    {
                        log?.Info("Unable to find the code action to execute for: " + args.CodeActionSuggestion.Description);
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            foreach (var action in actions)
            {
                try
                {
                    var suggestions = action.Suggest(context, location);

                    if (suggestions != null && suggestions.Any())
                    {
                        fixes.AddRange(suggestions.Select(s =>
                        {
                            return new ClassifiedTextRun(PredefinedClassificationTypeNames.Text,
                                                         "\n • " + s.Description,
                                                         () => codeFixClicked(new CodeIssueFixSelectedEventArgs(s)),
                                                         tooltip:null,
                                                         ClassifiedTextRunStyle.Underline);
                        }));
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            return fixes;
        }

        void Track(ICodeIssue issue)
        {
            try
            {
                var analyser = codeAnalysers.GetByIdentifier(issue.Identifier);

                if (analyser != null)
                {
                    analyticsService.Track(analyser.Name + "(" + analyser.DiagnosticId + ")");
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}