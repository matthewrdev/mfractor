using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Fonts.WorkUnits;
using MFractor.Maui;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ImportFontAssetCompletionService : IXamlCompletionService
    {
        readonly Lazy<IAnalyticsService> analyticsService;
        public IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Import Font Asset Completion";

        [ImportingConstructor]
        public ImportFontAssetCompletionService(Lazy<IAnalyticsService> analyticsService,
                                                Lazy<IWorkEngine> workEngine)
        {
            this.analyticsService = analyticsService;
            this.workEngine = workEngine;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var isFontAsset = IsFontAssetCompletion(context);

            return isFontAsset;
        }

        bool IsFontAssetCompletion(IXamlFeatureContext context)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null || attribute.Name.FullName != "FontFamily")
            {
                return false;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(attribute);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);

            return returnType.SpecialType == SpecialType.System_String;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return ProvideFontActionCompletions(textView, context);
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideFontActionCompletions(ITextView textView, IXamlFeatureContext context)
        {
            var items = new List<ICompletionSuggestion>();

            items.Add(CreateNativeImportFontCompletion(textView, context));

            var exportFontAttribute = context.Compilation.GetTypeByMetadataName(context.Platform.ExportFontAttribute.MetaType);
            if (exportFontAttribute != null)
            {

            }

            return items;
        }

        private CompletionSuggestion CreateNativeImportFontCompletion(ITextView textView, IXamlFeatureContext context)
        {
            var actionName = "Import a font into mobile projects";

            var action = new CompletionSuggestion(actionName, new ImageElement(IconIds.Wand));
            action.AddProperty(XamlCompletionItemPropertyKeys.CompletionAction, new CompletionAction((tv, t, ci) =>
            {
                void onFontImported(FontImportResult result)
                {
                    var span = TextEditorHelper.GetAttributeSpanAtOffset(textView.TextBuffer, tv.GetCaretOffset());

                    var workUnit = new ReplaceTextWorkUnit()
                    {
                        Span = span,
                        Text = "{" + context.Platform.StaticResourceExtension.MarkupExpressionName + " " + result.ResourceKey + "}",
                        FilePath = context.Document.FilePath,
                    };

                    WorkEngine.ApplyAsync(workUnit);
                }

                var importFontWorkUnit = new ImportFontWorkUnit
                {
                    Solution = context.Solution,
                    InjectFontIntoXaml = true,
                    ImportAction = onFontImported
                };

                AnalyticsService.Track("Import Font Asset");

                return importFontWorkUnit.AsList();

            }));

            action.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Import a new font into the Android, iOS and UWP projects that reference " + context.Project?.Name);
            action.AddProperty(XamlCompletionItemPropertyKeys.AnalyticsEvent, "Import Font Completion");
            return action;
        }
    }
}
