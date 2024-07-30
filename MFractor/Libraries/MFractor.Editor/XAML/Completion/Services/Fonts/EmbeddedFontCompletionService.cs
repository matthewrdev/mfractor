using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Maui.Fonts;
using MFractor.Maui.Syntax;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class EmbeddedFontCompletionService : IXamlCompletionService
    {
        readonly Lazy<IEmbeddedFontsResolver> embeddedFontsResolver;
        public IEmbeddedFontsResolver EmbeddedFontsResolver => embeddedFontsResolver.Value;

        [ImportingConstructor]
        public EmbeddedFontCompletionService(Lazy<IEmbeddedFontsResolver> embeddedFontsResolver)
        {
            this.embeddedFontsResolver = embeddedFontsResolver;
        }

        public string AnalyticsEvent => "Embedded Font Asset Completion";

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var isFontAsset = IsFontAssetCompletion(context, textView, triggerLocation);

            return isFontAsset;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var availableFonts = EmbeddedFontsResolver.GetEmbeddedFonts(context.Project, context.Platform);

            if (availableFonts == null || !availableFonts.Any())
            {
                return Array.Empty<ICompletionSuggestion>();
            }

            var items = new List<ICompletionSuggestion>();

            foreach (var font in availableFonts)
            {
                if (font.HasAlias)
                {
                    var item = new CompletionSuggestion(font.Alias + " (Embedded Font)", font.Alias);
                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Use the embedded font " + font.FontFileName + " with the '" + font.Alias + "' alias.");
                    items.Add(item);
                }
                else
                {
                    var item = new CompletionSuggestion(font.FontName + " (Embedded Font)", font.FontName);
                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipText, "Use the embedded font " + font.FontFileName);
                    items.Add(item);
                }
            }

            return items;
        }

        bool IsFontAssetCompletion(IXamlFeatureContext context, ITextView textView, SnapshotPoint triggerLocation)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null || attribute.Name.FullName != "FontFamily")
            {
                return false;
            }

            if (!CompletionHelper.IsWithinAttributeValue(context, textView, triggerLocation))
            {
                return false;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(attribute);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);

            return returnType.SpecialType == SpecialType.System_String;
        }
    }
}
