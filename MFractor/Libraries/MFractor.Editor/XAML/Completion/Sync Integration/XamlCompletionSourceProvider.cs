using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Analytics;
using MFractor.Editor.Utilities;
using MFractor.Maui;
using MFractor.Ide;
using MFractor.Licensing;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace MFractor.Editor.XAML.Completion.SyncIntegration
{
#if VS_WINDOWS
   using Microsoft.VisualStudio.Utilities;
   [Export(typeof(ICompletionSourceProvider))]
   [Name("MFractor Xaml Completion")]
   [ContentType(ContentTypes.Xaml)]
   [Order(After = "default")]
#endif
    class XamlCompletionSourceProvider : ICompletionSourceProvider
    {
        [ImportingConstructor]
        public XamlCompletionSourceProvider(ITextStructureNavigatorSelectorService structureNavigatorSelector,
                                            IXamlFeatureContextService xamlFeatureContextFactory,
                                            IXamlCompletionServiceRepository xamlCompletionServiceRepository,
                                            IAnalyticsService analyticsService,
                                            IXmlSyntaxFinder xmlSyntaxFinder,
                                            IIdeFeatureSettings featureSettings,
                                            ITextDocumentFactoryService textDocumentFactory,
                                            ILicensingService licensingService,
                                            ILicenseStatus licenseStatus,
                                            IWorkspaceService workspaceService)
        {
            this.structureNavigatorSelector = structureNavigatorSelector;
            this.xamlFeatureContextFactory = xamlFeatureContextFactory;
            this.xamlCompletionServiceRepository = xamlCompletionServiceRepository;
            this.analyticsService = analyticsService;
            this.xmlSyntaxFinder = xmlSyntaxFinder;
            this.featureSettings = featureSettings;
            this.textDocumentFactory = textDocumentFactory;
            this.licensingService = licensingService;
            this.licenseStatus = licenseStatus;
            this.workspaceService = workspaceService;
        }

        readonly ITextStructureNavigatorSelectorService structureNavigatorSelector;

        readonly IXamlFeatureContextService xamlFeatureContextFactory;

        readonly IXamlCompletionServiceRepository xamlCompletionServiceRepository;

        readonly IAnalyticsService analyticsService;

        readonly IXmlSyntaxFinder xmlSyntaxFinder;

        readonly IIdeFeatureSettings featureSettings;

        readonly ITextDocumentFactoryService textDocumentFactory;

        readonly ILicensingService licensingService;

        readonly ILicenseStatus licenseStatus;

        readonly IWorkspaceService workspaceService;

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            if (!featureSettings.UseXAMLIntelliSense)
            {
                return null;
            }

            if (!textDocumentFactory.TryGetTextDocument(textBuffer, out var textDocument))
            {
                return null;
            }

            if (!Path.GetExtension(textDocument.FilePath).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var project = TextBufferHelper.GetCompilationProject(textBuffer);
            if (project == null)
            {
                return null;
            }

            var source = new XamlCompletionSource(structureNavigatorSelector, xamlFeatureContextFactory, xamlCompletionServiceRepository, xmlSyntaxFinder, licensingService, analyticsService, workspaceService, licenseStatus, project.Id, textDocument.FilePath);
            
            return source;
        }
    }
}
