using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Editor.Tooltips;
using MFractor.Localisation;
using MFractor.Localisation.Tooltips;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace MFractor.Editor.CSharp.Tooltips
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class LocalisationTooltipQuickInfoSource : IAsyncQuickInfoSource
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly IAnalyticsService analyticsService;
        readonly IWorkspaceService workspaceService;
        readonly ILocalisationResolver localisationResolver;
        readonly ILocalisationTooltipRenderer localisationTooltipRenderer;
        string filePath;
        ProjectId projectId;

        [ImportingConstructor]
        public LocalisationTooltipQuickInfoSource(IAnalyticsService analyticsService,
                                            IWorkspaceService workspaceService,
                                            ILocalisationResolver localisationResolver,
                                            ILocalisationTooltipRenderer localisationTooltipRenderer)
        {
            this.analyticsService = analyticsService;
            this.workspaceService = workspaceService;
            this.localisationResolver = localisationResolver;
            this.localisationTooltipRenderer = localisationTooltipRenderer;
        }

        public void Initialise(string filePath, ProjectId projectId)
        {
            this.filePath = filePath;
            this.projectId = projectId;
        }

        public SyntaxTree SyntaxTreeForDocument(Document analysisDocument)
        {
            if (analysisDocument == null)
            {
                return null;
            }

            if (!analysisDocument.TryGetSyntaxTree(out var syntaxTree))
            {
                return null;
            }

            return syntaxTree;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session,
                                                         CancellationToken cancellationToken)
        {
            try
            {
                var project = workspaceService.CurrentWorkspace.CurrentSolution.GetProject(projectId);
                if (project is null
                    || !project.TryGetCompilation(out var compilation))
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var analysisDocument = project.Documents.FirstOrDefault(d => d.FilePath == filePath);
                if (analysisDocument is null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var syntaxTree = SyntaxTreeForDocument(analysisDocument);
                if (syntaxTree is null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var textBuffer = session.TextView.TextBuffer;
                var triggerPoint = session.GetTriggerPoint(textBuffer);

                var position = triggerPoint.GetPosition(textBuffer.CurrentSnapshot);
                var syntax = GetSyntaxAtLocation(syntaxTree, position);

                if (syntax is null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var symbolInfo = semanticModel.GetSymbolInfo(syntax);
                var property = symbolInfo.Symbol as IPropertySymbol;
                if (property is null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var localisations = localisationResolver.ResolveLocalisations(project, property);

                if (localisations is null)
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                var trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(Span.FromBounds(syntax.Span.Start, syntax.Span.End), SpanTrackingMode.EdgeInclusive);

                var tooltip = localisationTooltipRenderer.CreateLocalisationTooltip(localisations);

                if (string.IsNullOrEmpty(tooltip))
                {
                    return Task.FromResult<QuickInfoItem>(null);
                }

                analyticsService.Track("C# Localisation Tooltip");

                var tooltipModel = new TextContentTooltipModel()
                {
                    Content = tooltip,
                    NavigationContext = new Navigation.NavigationContext(filePath, project, position, new InteractionLocation(position)),
                    NavigationSuggestion = new Navigation.NavigationSuggestion("Navigate To Localisation", string.Empty, typeof(Localisation.Navigation.CSharpToResxNavigationHandler)),
                };

                return Task.FromResult(new QuickInfoItem(trackingSpan, tooltipModel.AsTextElement()));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        public ExpressionSyntax GetSyntaxAtLocation(SyntaxTree syntaxTree, int offset)
        {
            var syntaxRoot = syntaxTree.GetRoot();

            var span = TextSpan.FromBounds(offset, offset + 1);

            SyntaxNode node = null;
            try
            {
                if (syntaxRoot.Span.IntersectsWith(span) && span.Start >= syntaxRoot.SpanStart && span.End <= syntaxRoot.Span.End)
                {
                    node = syntaxRoot.FindNode(span);
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                log?.Info(aex.ToString());
            }
            catch (Exception ex)
            {
                log?.Warning(ex.ToString());
            }

            if (node is ExpressionSyntax expressionSyntax)
            {
                return expressionSyntax;
            }

            return default;
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}