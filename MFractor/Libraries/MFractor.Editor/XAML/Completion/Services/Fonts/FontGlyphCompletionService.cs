using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using MFractor.Editor.Tooltips;
using MFractor.Editor.Utilities;
using MFractor.Fonts;
using MFractor.Maui;
using MFractor.Maui.Fonts;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MFractor.Editor.XAML.Completion.Services.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class FontGlyphCompletionService : IXamlCompletionService
    {
        public string AnalyticsEvent => "Font Glyph Completion";

        readonly Lazy<IFontFamilyResolver> fontFamilyResolver;
        public IFontFamilyResolver FontFamilyResolver => fontFamilyResolver.Value;

        readonly Lazy<IFontService> fontService;
        public IFontService FontService => fontService.Value;

        [ImportingConstructor]
        public FontGlyphCompletionService(Lazy<IFontFamilyResolver> fontFamilyResolver,
                                          Lazy<IFontService> fontService)
        {
            this.fontFamilyResolver = fontFamilyResolver;
            this.fontService = fontService;
        }

        public bool CanProvideCompletions(ITextView textView, IXamlFeatureContext context, XamlExpressionSyntaxNode xamlExpression, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            var attribute = context.GetSyntax<XmlAttribute>();
            if (attribute == null)
            {
                return false;
            }

            var preceeding = textView.TextBuffer.GetFirstNonWhitespacePreceedingCharacter(triggerLocation.Position);
            if (preceeding != "\"")
            {
                return false;
            }

            if (ExpressionParserHelper.IsExpression(attribute.Value.Value))
            {
                return false;
            }

            var property = context.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property == null || property.Type.SpecialType != SpecialType.System_String)
            {
                return false;
            }

            var fontFamily = FontFamilyResolver.ResolveFont(attribute.Parent, context.XamlSemanticModel, context.Platform, context.XamlDocument.ProjectFile);
            if (fontFamily == null)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<ICompletionSuggestion> ProvideCompletions(ITextView textView,
                                                                     IXamlFeatureContext context,
                                                                     XamlExpressionSyntaxNode xamlExpression,
                                                                     SnapshotPoint triggerLocation,
                                                                     SnapshotSpan applicableToSpan,
                                                                     CancellationToken token)
        {
            var result = new List<ICompletionSuggestion>();
            var attribute = context.GetSyntax<XmlAttribute>();

            var font = FontFamilyResolver.ResolveFont(attribute.Parent, context.XamlSemanticModel, context.Platform, context.XamlDocument.ProjectFile);
            var typeface = FontService.GetFontTypeface(font);

            foreach (var glyph in typeface.GlyphCollection)
            {
                if (glyph.IsIcon && glyph.HasName)
                {
                    var item = new CompletionSuggestion(glyph.Name + $" ({glyph.CharacterCodeHex})", "&#x" + glyph.CharacterCodeHex + ";");

                    item.AddProperty(XamlCompletionItemPropertyKeys.TooltipModel, new FontGlyphTooltipModel(font, glyph.Unicode));

                    result.Add(item);
                }
            }

            return result;
        }
    }
}
