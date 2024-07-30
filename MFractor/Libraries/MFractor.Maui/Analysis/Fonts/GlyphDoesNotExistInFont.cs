using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Fonts.Utilities;
using MFractor.Maui.Fonts;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Xml;

namespace MFractor.Maui.Analysis
{
    class GlyphDoesNotExistInFont : XamlCodeAnalyser
    {
        readonly Lazy<IFontFamilyResolver> fontFamilyResolver;
        public IFontFamilyResolver FontFamilyResolver => fontFamilyResolver.Value;

        public override string Documentation => "When a font is applied to a XAML element via the FontFamily attribute, this analyser inspects the character code provided and verifies that it exists in the referenced font asset.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.glyph_does_not_exist_in_fonts";

        public override string Name => "Glyph Does Not Exist In Font";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1091";

        [ImportingConstructor]
        public GlyphDoesNotExistInFont(Lazy<IFontFamilyResolver> fontFamilyResolver)
        {
            this.fontFamilyResolver = fontFamilyResolver;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!syntax.HasValue)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return Array.Empty<ICodeIssue>();
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax);

            var returnType = SymbolHelper.ResolveMemberReturnType(symbol);

            if (returnType == null || returnType.SpecialType != Microsoft.CodeAnalysis.SpecialType.System_String)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (!FontGlyphCodeHelper.TryEscapedUnicodeCharacterToGlyphCodePoint(syntax.Value.Value, out var codepoint))
            {
                return Array.Empty<ICodeIssue>();
            }

            var font = FontFamilyResolver.ResolveFont(syntax.Parent, context.XamlSemanticModel, context.Platform, document.ProjectFile);

            if (font == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (!TryGetPreprocessor<FontTypefacePreprocessor>(context, out var preprocessor))
            {
                return Array.Empty<ICodeIssue>();
            }

            var typeface = preprocessor.GetFontTypeface(font);
            if (typeface == null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var glyph = typeface.GlyphCollection.GetGlyphByCodePoint(codepoint);
            if (glyph != null)
            {
                return Array.Empty<ICodeIssue>();
            }

            var message = $"{font.Name} does not contain a glyph for {syntax.Value.Value}.";

            return CreateIssue(message, syntax, syntax.Value.Span).AsList();
        }
    }
}

