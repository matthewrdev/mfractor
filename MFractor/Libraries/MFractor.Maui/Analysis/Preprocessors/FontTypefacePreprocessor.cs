using System.Collections.Generic;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Fonts;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// A <see cref="ICodeAnalysisPreprocessor"/> implementation that caches the <see cref="StaticResourceDefinition"/>'s available to the current document.
    /// </summary>
    public class FontTypefacePreprocessor : ICodeAnalysisPreprocessor
    {
        public FontTypefacePreprocessor(IFontService fontService)
        {
            this.fontService = fontService;
        }

        readonly IFontService fontService;

        readonly Dictionary<string, IFontTypeface> fontTypeFaces = new Dictionary<string, IFontTypeface>();

        public bool IsValid => true;

        public bool Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            return true;
        }

        public IFontTypeface GetFontTypeface(IFont font)
        {
            if (font == null || string.IsNullOrEmpty(font.PostscriptName))
            {
                return null;
            }

            if (fontTypeFaces.ContainsKey(font.PostscriptName))
            {
                return fontTypeFaces[font.PostscriptName];
            }

            var typeface = fontService.GetFontTypeface(font);

            fontTypeFaces[font.PostscriptName] = typeface; // Delibrate non-null check.

            return typeface;
        }
    }
}
