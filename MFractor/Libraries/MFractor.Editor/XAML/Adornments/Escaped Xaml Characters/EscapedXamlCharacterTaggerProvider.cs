using System;
using System.ComponentModel.Composition;
using MFractor.Editor.Adornments;
using MFractor.Ide;
using MFractor.Licensing;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Adornments.EscapedXamlCharacters
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypes.Xaml)]
    [TagType(typeof(EscapedCharacterTag))]
    sealed class EscapedXamlCharacterTaggerProvider : ITaggerProvider
    {
        readonly Lazy<ITextDocumentFactoryService> textDocumentFactory;
        public ITextDocumentFactoryService TextDocumentFactory => textDocumentFactory.Value;

        readonly Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService;
        public IXmlSyntaxTreeService XmlSyntaxTreeService => xmlSyntaxTreeService.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IIdeFeatureSettings> featureSettings;
        public IIdeFeatureSettings FeatureSettings => featureSettings.Value;

        readonly Lazy<IBufferTagAggregatorFactoryService> bufferTagAggregatorFactoryService;
        public IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService => bufferTagAggregatorFactoryService.Value;

        [ImportingConstructor]
        internal EscapedXamlCharacterTaggerProvider(Lazy<ITextDocumentFactoryService> textDocumentFactory,
                                                    Lazy<ILicensingService> licensingService,
                                                    Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                                    Lazy<IIdeFeatureSettings> featureSettings,
                                                    Lazy<IBufferTagAggregatorFactoryService> bufferTagAggregatorFactoryService)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.featureSettings = featureSettings;
            this.bufferTagAggregatorFactoryService = bufferTagAggregatorFactoryService;
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

            return buffer.Properties.GetOrCreateSingletonProperty(() => new EscapedXamlCharacterTagger(textDocument.FilePath, XmlSyntaxTreeService)) as ITagger<T>;
        }
    }
}