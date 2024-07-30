using System;
using System.ComponentModel.Composition;
using MFractor.Editor.Adornments;
using MFractor.Editor.Utilities;
using MFractor.Ide;
using MFractor.Licensing;
using MFractor.Maui.Grids;
using MFractor.Maui.XamlPlatforms;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Adornments.Grids
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypes.Xaml)]
    [TagType(typeof(GridIndexTag))]
    sealed class GridIndexTaggerProvider : ITaggerProvider
    {
        readonly Lazy<ITextDocumentFactoryService> textDocumentFactory;
        public ITextDocumentFactoryService TextDocumentFactory => textDocumentFactory.Value;

        readonly Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService;
        public IXmlSyntaxTreeService XmlSyntaxTreeService => xmlSyntaxTreeService.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IIdeFeatureSettings> featureSettings;
        public IIdeFeatureSettings FeatureSettings => featureSettings.Value;

        readonly Lazy<IGridAxisResolver> gridAxisResolver;
        public IGridAxisResolver GridAxisResolver => gridAxisResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        internal GridIndexTaggerProvider(Lazy<ITextDocumentFactoryService> textDocumentFactory,
                                         Lazy<ILicensingService> licensingService,
                                         Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                         Lazy<IIdeFeatureSettings> featureSettings,
                                         Lazy<IGridAxisResolver> gridAxisResolver,
                                         Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.featureSettings = featureSettings;
            this.gridAxisResolver = gridAxisResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!FeatureSettings.AllowGridAdornments)
            {
                return null;
            }

            if (!TextDocumentFactory.TryGetTextDocument(buffer, out var textDocument))
            {
                return null;
            }

            if (!LicensingService.IsPaid)
            {
                return null;
            }

            return buffer.Properties.GetOrCreateSingletonProperty(() => new GridIndexTagger(textDocument.FilePath,
                                                                                            XmlSyntaxTreeService,
                                                                                            XamlPlatforms,
                                                                                            GridAxisResolver)) as ITagger<T>;
        }
    }
}
