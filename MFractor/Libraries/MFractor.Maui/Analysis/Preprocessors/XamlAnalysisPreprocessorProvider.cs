using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Code.Documents;
using MFractor.Fonts;
using MFractor.Maui.Fonts;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Styles;
using MFractor.Images;
using MFractor.Workspace.Data;
using MFractor.Code;

namespace MFractor.Maui.Analysis.Preprocessors
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class XamlAnalysisPreprocessorProvider : ICodeAnalysisPreprocessorProvider
    {
        readonly IStaticResourceResolver staticResourceResolver;
        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;
        readonly IImageAssetService imageAssetService;
        readonly IStyleResolver styleResolver;
        readonly IFontService fontService;
        readonly IEmbeddedFontsResolver embeddedFontsResolver;

        [ImportingConstructor]
        public XamlAnalysisPreprocessorProvider(IStaticResourceResolver staticResourceResolver,
                                                IResourcesDatabaseEngine resourcesDatabaseEngine,
                                                IImageAssetService imageAssetService,
                                                IStyleResolver styleResolver,
                                                IFontService fontService,
                                                IEmbeddedFontsResolver embeddedFontsResolver)
        {
            this.staticResourceResolver = staticResourceResolver;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.imageAssetService = imageAssetService;
            this.styleResolver = styleResolver;
            this.fontService = fontService;
            this.embeddedFontsResolver = embeddedFontsResolver;
        }

        public IEnumerable<ICodeAnalysisPreprocessor> ProvidePreprocessors(IParsedXmlDocument document, IFeatureContext context)
        {
            var xamlContext = context as IXamlFeatureContext;
            var xamlDocument = document as IParsedXamlDocument;

            if (xamlContext == null || xamlDocument == null)
            {
                return Enumerable.Empty<ICodeAnalysisPreprocessor>();
            }

            return new List<ICodeAnalysisPreprocessor>()
            {
                new StaticResourceAnalysisPreprocessor(staticResourceResolver, resourcesDatabaseEngine),
                new ImageResourceAnalysisPreprocessor(imageAssetService),
                new StyleAnalysisPreprocessor(styleResolver, resourcesDatabaseEngine),
                new FontTypefacePreprocessor(fontService),
                new ExportedFontPreprocessor(embeddedFontsResolver)
            };
        }
    }
}
