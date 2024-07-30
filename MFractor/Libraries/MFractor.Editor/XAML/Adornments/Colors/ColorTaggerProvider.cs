using System;
using System.ComponentModel.Composition;
using MFractor.Data;
using MFractor.Editor.Adornments;
using MFractor.Ide;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.Editor.XAML.Adornments.Colors
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(ContentTypes.Xaml)]
    [TagType(typeof(ColorTag))]
    sealed class ColorTaggerProvider : ITaggerProvider
    {
        readonly Lazy<ITextDocumentFactoryService> textDocumentFactory;
        public ITextDocumentFactoryService TextDocumentFactory => textDocumentFactory.Value;

        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IIdeFeatureSettings> featureSettings;
        public IIdeFeatureSettings FeatureSettings => featureSettings.Value;

        [ImportingConstructor]
        internal ColorTaggerProvider(Lazy<ITextDocumentFactoryService> textDocumentFactory,
                                     Lazy<ILicensingService> licensingService,
                                     Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                     Lazy<IWorkspaceService> workspaceService,
                                     Lazy<IIdeFeatureSettings> featureSettings,
                                     Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                     Lazy<IWorkEngine> workEngine)
        {
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
            this.featureSettings = featureSettings;
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (!FeatureSettings.AllowColorAdornments)
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

            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            return buffer.Properties.GetOrCreateSingletonProperty(() =>
            {
                return Resolver.Resolve<ColorTagger>() as ITagger<T>;
            });
        }
    }
}