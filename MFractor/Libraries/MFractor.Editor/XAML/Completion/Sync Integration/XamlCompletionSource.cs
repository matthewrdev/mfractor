using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Media.Imaging;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Licensing;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using CompletionItem = Microsoft.VisualStudio.Language.Intellisense.Completion;

namespace MFractor.Editor.XAML.Completion.SyncIntegration
{
    class XamlCompletionSource : ICompletionSource, IAnalyticsFeature
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly ITextStructureNavigatorSelectorService structureNavigatorSelector;
        readonly IXamlCompletionServiceRepository xamlCompletionServiceRepository;
        readonly IXmlSyntaxFinder xmlSyntaxFinder;
        readonly ILicensingService licensingService;
        readonly IWorkspaceService workspaceService;
        readonly ProjectId projectId;
        readonly string filePath;
        readonly ILicenseStatus licenseStatus;
        readonly IntelliSenseAnalyticsTracker sessionTracker;
        readonly IXamlFeatureContextService xamlFeatureContextFactory;

        private static BitmapImage _icon;

        private static BitmapImage Icon
        {
            get
            {
                if (_icon == null)
                {
                    try
                    {
                        var bitmap = new BitmapImage();
                        using (var stream = Assembly.GetAssembly(typeof(XamlCompletionSource)).GetManifestResourceStream("mfractor_logo_32.png"))
                        {
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
#if VS_WINDOWS
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
#endif
                            bitmap.EndInit();
                        }

                        _icon = bitmap;
                    }
                    catch
                    {
                    }
                }
                return _icon;
            }
        }

        public XamlCompletionSource(ITextStructureNavigatorSelectorService structureNavigatorSelector,
                                    IXamlFeatureContextService xamlFeatureContextFactory,
                                    IXamlCompletionServiceRepository xamlCompletionServiceRepository,
                                    IXmlSyntaxFinder xmlSyntaxFinder,
                                    ILicensingService licensingService,
                                    IAnalyticsService analyticsService,
                                    IWorkspaceService workspaceService,
                                    ILicenseStatus licenseStatus,
                                    ProjectId projectId,
                                    string filePath)
        {
            this.structureNavigatorSelector = structureNavigatorSelector;
            this.xamlFeatureContextFactory = xamlFeatureContextFactory;
            this.xamlCompletionServiceRepository = xamlCompletionServiceRepository;
            this.xmlSyntaxFinder = xmlSyntaxFinder;
            this.licensingService = licensingService;
            this.workspaceService = workspaceService;
            this.licenseStatus = licenseStatus;
            this.projectId = projectId;
            this.filePath = filePath;
            sessionTracker = new IntelliSenseAnalyticsTracker(analyticsService, this);
        }

        public string AnalyticsEvent => "XAML IntelliSense";

        public void AugmentCompletionSession(ICompletionSession session,
                                             IList<CompletionSet> completionSets)
        {
            if (!licensingService.IsPaid)
            {
                return;
            }

            var textBuffer = session.TextView.TextBuffer;
            var triggerSpan = session.GetTriggerPoint(textBuffer);
            var triggerLocation = triggerSpan.GetPoint(textBuffer.CurrentSnapshot);
            var applicableToSpan = TextEditorHelper.FindTokenSpanAtPosition(textBuffer, triggerLocation.Position, structureNavigatorSelector);
            var token = CancellationToken.None;

            try
            {
                var project = workspaceService.CurrentWorkspace.CurrentSolution.GetProject(projectId);
                if (project == null)
                {
                    return;
                }

                var context = xamlFeatureContextFactory.CreateXamlFeatureContextAsync(project, filePath, triggerLocation.Position, token).Result;
                if (context == null)
                {
                    return;
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
                        return;
                    }
                }
                else if (last is XmlAttribute xmlAttribute
                    && xmlAttribute.HasValue
                    && ExpressionParserHelper.IsExpression(xmlAttribute.Value.Value))
                {
                    var attributeSpan = TextEditorHelper.GetAttributeSpanAtOffset(session.TextView.TextBuffer, triggerLocation.Position);
                    expression = new XamlExpressionParser(xmlAttribute.Value.Value, attributeSpan.Start).Parse();
                }

                var services = xamlCompletionServiceRepository.GetAvailableServices(session.TextView, context, expression, triggerLocation, applicableToSpan, token);
                token.ThrowIfCancellationRequested();

                foreach (var service in services)
                {
                    try
                    {
                        var completions = service.ProvideCompletions(session.TextView, context, expression, triggerLocation, applicableToSpan, token);

                        if (completions != null && completions.Any())
                        {
                            foreach (var completion in completions)
                            {
                                token.ThrowIfCancellationRequested();

                                if (completion.Properties.ContainsKey(XamlCompletionItemPropertyKeys.CompletionAction))
                                {
                                    // Ignore, completion actions are not available in the sync integration.
                                    continue;
                                }

                                var tooltip = completion.GetProperty<string>(XamlCompletionItemPropertyKeys.TooltipText) ?? string.Empty;

                                if (!tooltip.EndsWith(licenseStatus.BrandingText))
                                {
                                    if (!string.IsNullOrEmpty(tooltip))
                                    {
                                        tooltip += "\n\n";
                                    }

                                    tooltip += licenseStatus.BrandingText;
                                }

                                var item = new CompletionItem(completion.DisplayText, completion.Insertion, tooltip ?? completion.DisplayText, Icon, string.Empty);

                                foreach (var prop in completion.Properties)
                                {
                                    item.Properties.AddProperty(prop.Key, prop.Value);
                                }

                                if (!item.Properties.ContainsProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent))
                                {
                                    item.Properties.AddProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, service.AnalyticsEvent);
                                }

                                item.Properties.AddProperty(XamlCompletionItemPropertyKeys.CompletionSuggestion, completion);
                                completionItemsBuilder.Add(item);
                            }
                        }
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

                if (path.LastOrDefault() is XmlAttribute attribute
                    && attribute.HasValue
                    && expression == null)
                {
                    var value = attribute.Value.Value;

                    for (var i = completionItemsBuilder.Count; i > 0; --i)
                    {
                        var item = completionItemsBuilder[i - 1];

                        if (item.DisplayText.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) < 0)
                        {
                            completionItemsBuilder.RemoveAt(i - 1);
                        }
                    }
                }

                var trackingSpan = textBuffer.CurrentSnapshot.CreateTrackingSpan(applicableToSpan.Start, applicableToSpan.Length, SpanTrackingMode.EdgeInclusive, TrackingFidelityMode.Backward);

                var set = new CompletionSet("MFractor", "MFractor", trackingSpan, completionItemsBuilder.ToList(), null);

                if (completionItemsBuilder.Any())
                {
                    sessionTracker.TrackIntelliSenseSession();
                    session.Properties.AddProperty(XamlCompletionItemPropertyKeys.IsMFractorSession, true);
                    completionSets.Add(set);
                }
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

        public void Dispose()
        {
        }

        //public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        //{
        //    try
        //    {
        //        // We don't trigger completion when user typed
        //        if (IsIgnorablePunctuation(trigger)
        //            || trigger.Character == '\n'             // new line
        //            || trigger.Reason == CompletionTriggerReason.Backspace
        //            || trigger.Reason == CompletionTriggerReason.Deletion)
        //        {
        //            return CompletionStartData.DoesNotParticipateInCompletion;
        //        }

        //        var textBuffer = triggerLocation.Snapshot.TextBuffer;

        //        var tokenSpan = TextEditorHelper.FindTokenSpanAtPosition(textBuffer, triggerLocation.Position, structureNavigatorSelector);
        //        return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
        //    }
        //    catch (Exception ex)
        //    {
        //        log?.Exception(ex);
        //    }

        //    return CompletionStartData.DoesNotParticipateInCompletion;
        //}

        //bool IsIgnorablePunctuation(CompletionTrigger trigger)
        //{
        //    if (char.IsPunctuation(trigger.Character))
        //    {
        //        switch (trigger.Character)
        //        {
        //            case '*': // * is a grid length value.
        //                return false;
        //            default:
        //                return true;
        //        }
        //    }

        //    if (trigger.Character == '\0'
        //        && trigger.Reason == CompletionTriggerReason.InvokeAndCommitIfUnique)
        //    {
        //        return false;
        //    }

        //    return false;
        //}
    }
}