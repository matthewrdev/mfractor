using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Ide;
using MFractor.Licensing;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;

namespace MFractor.Editor.XAML.Completion
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export]
    class XamlAsyncCompletionSource : IAsyncCompletionSource, IAnalyticsFeature
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly ITextStructureNavigatorSelectorService structureNavigatorSelector;
        readonly IWorkspaceService workspaceService;
        readonly IAsyncXamlCompletionServiceRepository asyncXamlCompletionServiceRepository;
        readonly IXmlSyntaxFinder xmlSyntaxFinder;
        readonly IIdeFeatureSettings featureSettings;
        readonly ILicenseStatus licenseStatus;
        readonly IntelliSenseAnalyticsTracker sessionTracker;
        readonly IXamlFeatureContextService xamlFeatureContextFactory;
        readonly IXamlCompletionServiceRepository xamlCompletionServiceRepository;

        string filePath;
        ProjectId projectId;

        [ImportingConstructor]
        public XamlAsyncCompletionSource(ITextStructureNavigatorSelectorService structureNavigatorSelector,
                                         IWorkspaceService workspaceService,
                                         IXamlFeatureContextService xamlFeatureContextFactory,
                                         IXamlCompletionServiceRepository xamlCompletionServiceRepository,
                                         IAsyncXamlCompletionServiceRepository asyncXamlCompletionServiceRepository,
                                         IXmlSyntaxFinder xmlSyntaxFinder,
                                         IIdeFeatureSettings featureSettings,
                                         ILicenseStatus licenseStatus,
                                         IAnalyticsService analyticsService)
        {
            this.structureNavigatorSelector = structureNavigatorSelector;
            this.workspaceService = workspaceService;
            this.xamlFeatureContextFactory = xamlFeatureContextFactory;
            this.xamlCompletionServiceRepository = xamlCompletionServiceRepository;
            this.asyncXamlCompletionServiceRepository = asyncXamlCompletionServiceRepository;
            this.xmlSyntaxFinder = xmlSyntaxFinder;
            this.featureSettings = featureSettings;
            this.licenseStatus = licenseStatus;

            sessionTracker = new IntelliSenseAnalyticsTracker(analyticsService, this);
        }

        public void Initialise(string filePath, ProjectId projectId)
        {
            this.filePath = filePath;
            this.projectId = projectId;
        }

        public string AnalyticsEvent => "XAML IntelliSense";

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session,
                                                                       CompletionTrigger trigger,
                                                                       SnapshotPoint triggerLocation,
                                                                       SnapshotSpan applicableToSpan,
                                                                       CancellationToken token)
        {
            try
            {
                if (!session.TextView.Selection.IsEmpty)
                {
                    return new CompletionContext(ImmutableArray<CompletionItem>.Empty);
                }

                var project = workspaceService.CurrentWorkspace.CurrentSolution.GetProject(projectId);
                if (project == null)
                {
                    return new CompletionContext(ImmutableArray<CompletionItem>.Empty);
                }

                var context = await xamlFeatureContextFactory.CreateXamlFeatureContextAsync(project, filePath, triggerLocation.Position, token);
                if (context == null)
                {
                    return new CompletionContext(ImmutableArray<CompletionItem>.Empty);
                }
                token.ThrowIfCancellationRequested();

                var completionItemsBuilder = ImmutableArray.CreateBuilder<CompletionItem>();
                var path = xmlSyntaxFinder.BuildXmlPathToOffset(context.SyntaxTree, triggerLocation.Position, out var span);

                XamlExpressionSyntaxNode expression = null;
                var last = path.LastOrDefault();
                if (last is XmlNode node)
                {
                    if (node.HasClosingTag && node.ClosingTagNameSpan.IntersectsWith(triggerLocation.Position))
                    {
                        return new CompletionContext(ImmutableArray<CompletionItem>.Empty);
                    }
                }
                else if (last is XmlAttribute xmlAttribute
                    && xmlAttribute.HasValue
                    && ExpressionParserHelper.IsExpression(xmlAttribute.Value.Value))
                {
                    var attributeSpan = TextEditorHelper.GetAttributeSpanAtOffset(session.TextView.TextBuffer, triggerLocation.Position);
                    expression = new XamlExpressionParser(xmlAttribute.Value.Value, attributeSpan.Start).Parse();
                }

                var syncServices = xamlCompletionServiceRepository.GetAvailableServices(session.TextView, context, expression, triggerLocation, applicableToSpan, token);

                foreach (var service in syncServices)
                {
                    try
                    {
                        var completions = service.ProvideCompletions(session.TextView, context, expression, triggerLocation, applicableToSpan, token);

                        BuildCompletions(completionItemsBuilder, service.AnalyticsEvent, completions, applicableToSpan, token);

                        token.ThrowIfCancellationRequested();
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

                var asyncServices = (await asyncXamlCompletionServiceRepository.GetAvailableServicesAsync(session.TextView, context, expression, triggerLocation, applicableToSpan, token)).ToList();
                
                token.ThrowIfCancellationRequested();

                foreach (var service in asyncServices)
                {
                    try
                    {
                        var completions = await service.ProvideCompletionsAsync(session.TextView, context, expression, triggerLocation, applicableToSpan, token);

                        BuildCompletions(completionItemsBuilder, service.AnalyticsEvent, completions, applicableToSpan, token);

                        token.ThrowIfCancellationRequested();
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

                FilterCompletions(completionItemsBuilder, path, expression);

                var completionItems = new CompletionContext(completionItemsBuilder.ToImmutable());

                if (completionItems.Items.Any())
                {
                    sessionTracker.TrackIntelliSenseSession();
                    session.Properties.AddProperty(XamlCompletionItemPropertyKeys.IsMFractorSession, true);
                }

                return completionItems;
            }
            catch (OperationCanceledException oex)
            {
                throw oex;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return new CompletionContext(ImmutableArray<CompletionItem>.Empty);
        }

        void FilterCompletions(ImmutableArray<CompletionItem>.Builder completionItemsBuilder, IReadOnlyList<XmlSyntax> path, XamlExpressionSyntaxNode expression)
        {
            if (path.LastOrDefault() is XmlAttribute attribute
                && attribute.HasValue
                && expression == null)
            {
                var value = attribute.Value.Value;

                if (!attribute.Value.IsClosed)
                {
                    // Do not filter as the attribute does not have a closing " yet. We can likely still provide these suggestions.
                    return;
                }

                for (var i = completionItemsBuilder.Count; i > 0; --i)
                {
                    var item = completionItemsBuilder[i - 1];

                    if (item.DisplayText.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) < 0)
                    {
                        completionItemsBuilder.RemoveAt(i - 1);
                    }
                }
            }
        }

        void BuildCompletions(ImmutableArray<CompletionItem>.Builder completionItemsBuilder,
                             string analyticsEvent,
                             IReadOnlyList<ICompletionSuggestion> completions,
                             SnapshotSpan applicableToSpan,
                             CancellationToken token)
        {
            if (completions != null && completions.Any())
            {
                foreach (var completion in completions)
                {
                    token.ThrowIfCancellationRequested();

#if VS_MAC
                    var item = new CompletionItem(completion.DisplayText, this, completion.Icon, ImmutableArray<CompletionFilter>.Empty, string.Empty, completion.Insertion, completion.DisplayText, completion.DisplayText, completion.DisplayText, ImmutableArray<ImageElement>.Empty, ImmutableArray<char>.Empty, applicableToSpan, false, false);
#else
                    var item = new CompletionItem(completion.DisplayText, this, completion.Icon, ImmutableArray<CompletionFilter>.Empty, string.Empty, completion.Insertion, completion.DisplayText, completion.DisplayText, ImmutableArray<ImageElement>.Empty);
#endif

                    if (completion.Properties.Any())
                    {
                        foreach (var prop in completion.Properties)
                        {
                            item.Properties.AddProperty(prop.Key, prop.Value);

                        }
                    }

                    if (!item.Properties.ContainsProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent))
                    {
                        item.Properties.AddProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, analyticsEvent);
                    }

                    item.Properties.AddProperty(XamlCompletionItemPropertyKeys.CompletionSuggestion, completion);
                    completionItemsBuilder.Add(item);
                }
            }
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (item.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.TooltipModel, out object tooltipModel))
            {
                return Task.FromResult<object>(tooltipModel);
            }

            if (item.Properties.TryGetProperty(XamlCompletionItemPropertyKeys.TooltipText, out string content))
            {
                content = content.Replace(CompletionHelper.CaretLocationMarker, string.Empty) + "\n\n" + licenseStatus.BrandingText;

                return Task.FromResult<object>(content);
            }

            return Task.FromResult<object>(default);
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            try
            {
                if (!featureSettings.UseXAMLIntelliSense)
                {
                    return CompletionStartData.DoesNotParticipateInCompletion;
                }

                // We don't trigger completion when user typed
                if (IsIgnorablePunctuation(trigger)
                    || trigger.Character == '\n'             // new line
                    || trigger.Character == '\t'             // tab
                    || trigger.Reason == CompletionTriggerReason.Backspace
                    || trigger.Reason == CompletionTriggerReason.Deletion)
                {
                    return CompletionStartData.DoesNotParticipateInCompletion;
                }

                var tokenSpan = TextEditorHelper.FindTokenSpanAtPosition(triggerLocation, structureNavigatorSelector);
                return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return CompletionStartData.DoesNotParticipateInCompletion;
        }

        bool IsIgnorablePunctuation(CompletionTrigger trigger)
        {
            if (char.IsPunctuation(trigger.Character))
            {
                switch (trigger.Character)
                {
                    case '*': // * is a grid length value.
                        return false;
                    case '\t':
                        return true;
                    case '"':
                        if (trigger.Reason == CompletionTriggerReason.Insertion)
                        {
                            return false;
                        }
                        return true;
                    default:
                        return true;
                }
            }

            if (trigger.Character == '\0'
                && trigger.Reason == CompletionTriggerReason.InvokeAndCommitIfUnique)
            {
                return false;
            }

            return false;
        }
    }
}
