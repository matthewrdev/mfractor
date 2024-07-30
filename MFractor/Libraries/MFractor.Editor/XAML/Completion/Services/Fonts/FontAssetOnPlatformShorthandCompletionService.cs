using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Fonts;
using MFractor.Code;
using MFractor.Maui;
using MFractor.Maui.CodeGeneration.Fonts;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using MFractor.Code.Formatting;

namespace MFractor.Editor.XAML.Completion.Services.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FontAssetOnPlatformShorthandCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Font Asset OnPlatform Shorthand Completion";

        readonly Lazy<IFontAssetResolver> fontAssetResolver;
        public IFontAssetResolver FontAssetResolver => fontAssetResolver.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IFontFamilyOnPlatformGenerator> fontFamilyOnPlatformGenerator;
        public IFontFamilyOnPlatformGenerator FontFamilyOnPlatformGenerator => fontFamilyOnPlatformGenerator.Value;

        [ImportingConstructor]
        public FontAssetOnPlatformShorthandCompletionService(Lazy<IFontAssetResolver> fontAssetResolver,
                                          Lazy<IWorkEngine> workEngine,
                                          Lazy<IFontFamilyOnPlatformGenerator> fontFamilyOnPlatformGenerator,
                                          Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                          Lazy<ICodeFormattingPolicyService> formattingPolicyService)
        {
            this.fontAssetResolver = fontAssetResolver;
            this.workEngine = workEngine;
            this.fontFamilyOnPlatformGenerator = fontFamilyOnPlatformGenerator;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var node = context.GetSyntax<XmlNode>();

            if (node == null || !node.HasParent)
            {
                return false;
            }

            if (!CompletionHelper.IsOnOpeningTag(node, textView.TextBuffer, triggerLocation.Position))
            {
                return false;
            }

            var parent = node.Parent;
            var type = context.XamlSemanticModel.GetSymbol(parent) as INamedTypeSymbol;

            var isCandidate = SymbolHelper.DerivesFrom(type, context.Platform.ResourceDictionary.MetaType);
            if (!isCandidate)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var items = new List<ICompletionSuggestion>();

            var fontAssets = FontAssetResolver.GetAvailableFontAssets(context.Project);

            foreach (var asset in fontAssets.DistinctBy(fa => fa.PostscriptName))
            {
                items.Add(CreateFontAssetCompletion(asset, textView.TextBuffer, triggerLocation));
            }

            return items;
        }

        ICompletionSuggestion CreateFontAssetCompletion(IFontAsset fontAsset, ITextBuffer textBuffer, SnapshotPoint triggerLocation)
        {
            var nodeName = textBuffer.GetCurrentNodeNameSpanAtOffset(triggerLocation.Position);
            var location = textBuffer.GetLocation(nodeName.Start);

            var code = FontFamilyOnPlatformGenerator.GenerateXaml(fontAsset, fontAsset.PostscriptName.FirstCharToLower(), new[] { PlatformFramework.Android, PlatformFramework.iOS }).Trim();

            if (code.StartsWith("<", StringComparison.Ordinal))
            {
                code = code.Substring(1, code.Length - 1);
            }

            var insertion = code.Replace("\n", "\n" + (new string(' ', location.Column - 1)));

            var item = new CompletionSuggestion(fontAsset.PostscriptName, insertion);
            item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, $"Create a font family declaration for {fontAsset.PostscriptName}.\n\nInserts:\n" + code);

            return item;
        }
    }
}
