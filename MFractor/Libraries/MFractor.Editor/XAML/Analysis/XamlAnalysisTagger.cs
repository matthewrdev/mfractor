using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Editor.Utilities;
using MFractor.Maui.Analysis;
using MFractor.Text;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.XAML.Analysis
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class XamlAnalysisTagger : ITagger<ErrorTag>, IDisposable
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        ITextView textView;
        string filePath;
        ITextBuffer textBuffer;

        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;
        readonly IWorkspaceService workspaceService;
        readonly object updateLock = new object();
        CancellationTokenSource tokenSource;
        readonly IAnalysisResultStore resultsStore;
        readonly IXamlAnalyser xamlAnalyser;
        readonly XamlAnalysisDebouncer xamlAnalysisDebouncer;

        readonly object versionNumberLock = new object();
        int? lastAnalysisVersionNumber = null;

        [ImportingConstructor]
        public XamlAnalysisTagger(IResourcesDatabaseEngine resourcesDatabaseEngine,
                                  IWorkspaceService workspaceService,
                                  IAnalysisResultStore resultsStore,
                                  IXamlAnalyser xamlAnalyser)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.workspaceService = workspaceService;
            this.resultsStore = resultsStore;
            this.xamlAnalyser = xamlAnalyser;

            this.xamlAnalysisDebouncer = new XamlAnalysisDebouncer();
        }

        public void Initialise(ITextView textView, string filePath)
        {
            this.textView = textView;
            this.textBuffer = textView.TextBuffer;
            this.filePath = filePath;

            workspaceService.CurrentWorkspace.WorkspaceChanged += CurrentWorkspace_WorkspaceChanged;
            textView.GotAggregateFocus += View_GotAggregateFocus;
            textBuffer.Changed += TextBuffer_Changed;
            resourcesDatabaseEngine.SolutionSyncEnded += ResourcesDatabaseEngine_SolutionSyncEnded;
            resourcesDatabaseEngine.ProjectSynchronisationPassCompleted += ResourcesDatabaseEngine_ProjectSynchronisationPassCompleted;
            xamlAnalyser.OnAnalysisCompleted += XamlAnalyser_OnAnalysiCompleted;
        }

        void XamlAnalyser_OnAnalysiCompleted(object sender, XamlAnalysisResultEventArgs e)
        {
            if (e.HasIssues)
            {
                resultsStore.Store(filePath, e.Issues);

                lock (updateLock)
                {
                    var snapshot = textView.TextBuffer.CurrentSnapshot;
                    TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
                }
            }
        }

        void ResourcesDatabaseEngine_ProjectSynchronisationPassCompleted(object sender, ProjectSynchronisationPassCompletedEventArgs e)
        {
            if (!textView.HasAggregateFocus)
            {
                return;
            }

            RunAnalysis(this.textView.TextBuffer.CurrentSnapshot, true);
        }

        void CurrentWorkspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            if (!textView.HasAggregateFocus)
            {
                return;
            }

            if (e.Kind == WorkspaceChangeKind.DocumentChanged)
            {
                var document = e.NewSolution?.GetDocument(e.DocumentId);

                if (document != null
                    && Path.GetExtension(document.FilePath) == ".cs"
                    && !document.FilePath.EndsWith(".g.cs"))
                {
                    RunAnalysis(this.textView.TextBuffer.CurrentSnapshot, true);
                }
            }
        }

        void ResourcesDatabaseEngine_SolutionSyncEnded(object sender, SolutionSynchronisationStatusEventArgs e)
        {
            RunAnalysis(this.textView.TextBuffer.CurrentSnapshot, true);
        }

        void View_GotAggregateFocus(object sender, EventArgs e)
        {
            RunAnalysis(this.textView.TextBuffer.CurrentSnapshot);
        }

        void TextBuffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            RunAnalysis(e.After.TextBuffer.CurrentSnapshot);
        }

        public void RunAnalysis(ITextSnapshot snapshot, bool force = false)
        {
            try
            {
                var currentVersion = snapshot.Version.VersionNumber;

                if (!force)
                {
                    lock (versionNumberLock)
                    {
                        if (lastAnalysisVersionNumber != null
                            && currentVersion == lastAnalysisVersionNumber)
                        {
                            return;
                        }

                        lastAnalysisVersionNumber = currentVersion;
                    }
                }
                else
                {
                    lastAnalysisVersionNumber = currentVersion;
                }

                resultsStore.Clear(filePath);
                lock (updateLock)
                {
                    TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(snapshot, 0, snapshot.Length)));
                }

                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource = null;
                }

                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;

                token.ThrowIfCancellationRequested();

                var project = TextBufferHelper.GetCompilationProject(snapshot.TextBuffer);

                if (project == null)
                {
                    return;
                }

                token.ThrowIfCancellationRequested();
                var content = snapshot.GetText();

                xamlAnalysisDebouncer.RequestAnalysis(xamlAnalyser, new StringTextProvider(content), filePath, project.Id, token);

            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {

            var issues = resultsStore.Retrieve(filePath)
                                     .Where(i => !i.IsSilent)
                                     .ToList();
            if (!issues.Any())
            {
                return Enumerable.Empty<ITagSpan<ErrorTag>>();
            }

            try
            {
                var tags = issues.Select(i =>
                {
                    var category = PredefinedErrorTypeNames.Warning;
                    switch (i.Classification)
                    {
                        case IssueClassification.Error:
                            category = PredefinedErrorTypeNames.CompilerError;
                            break;
                        case IssueClassification.Warning:
                        case IssueClassification.Improvement:
                            category = PredefinedErrorTypeNames.Warning;
                            break;
                    }

                    var start = i.Span.Start;
                    var end = i.Span.End;

                    if (start < 0 || start >= textBuffer.CurrentSnapshot.Length)
                    {
                        return default;
                    }

                    if (end < 0 || end >= textBuffer.CurrentSnapshot.Length)
                    {
                        return default;
                    }

                    var span = new SnapshotSpan(textBuffer.CurrentSnapshot, Span.FromBounds(start, end));

                    return new TagSpan<ErrorTag>(span, new ErrorTag(category));
                }).Where(t => t != null).ToList();

                return tags;
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Enumerable.Empty<ITagSpan<ErrorTag>>();
        }

        public void Dispose()
        {
            resultsStore.Clear(filePath);

            xamlAnalyser.OnAnalysisCompleted -= XamlAnalyser_OnAnalysiCompleted;
            xamlAnalyser.Dispose();
            workspaceService.CurrentWorkspace.WorkspaceChanged -= CurrentWorkspace_WorkspaceChanged;
            resourcesDatabaseEngine.SolutionSyncEnded -= ResourcesDatabaseEngine_SolutionSyncEnded;
            resourcesDatabaseEngine.ProjectSynchronisationPassCompleted -= ResourcesDatabaseEngine_ProjectSynchronisationPassCompleted;
            textView.GotAggregateFocus -= View_GotAggregateFocus;
            textBuffer.Changed -= TextBuffer_Changed;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}
