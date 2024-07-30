using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Maui.Fonts;

namespace MFractor.Maui.Analysis.Preprocessors
{
    /// <summary>
    /// An <see cref="ICodeAnalysisPreprocessor"/> that gathers the exported fonts for the current assembly.
    /// </summary>
    public class ExportedFontPreprocessor : ICodeAnalysisPreprocessor
    {
        readonly IEmbeddedFontsResolver embeddedFontsResolver;

        public bool IsValid
        {
            get;
            private set;
        }

        Lazy<IReadOnlyList<IEmbeddedFont>> embeddedFonts;
        public IReadOnlyList<IEmbeddedFont> EmbeddedFonts => embeddedFonts.Value;

        public ExportedFontPreprocessor(IEmbeddedFontsResolver embeddedFontsResolver)
        {
            this.embeddedFontsResolver = embeddedFontsResolver;
        }

        public bool Preprocess(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            if (xamlContext == null)
            {
                return false;
            }

            embeddedFonts = new Lazy<IReadOnlyList<IEmbeddedFont>>(() =>
            {
                return embeddedFontsResolver.GetEmbeddedFonts(context.Project, xamlContext.Platform).ToList();
            });

            IsValid = true;

            return true;
        }
    }
}