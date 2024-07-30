//using System;
//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using System.Linq;
//using System.Threading.Tasks;
//using MFractor.Images;
//using MFractor.Progress;
//using MFractor.Text;
//using MFractor.Utilities;
//using MFractor.Xml;

//namespace MFractor.Maui.Images
//{
//    class XamlImageAssetUsageFinder : IImageAssetUsageFinder
//    {
//        readonly Logging.ILogger log = Logging.Logger.Create();

//        public string[] SupportedFileExtensions { get; } = new string[] { ".xaml" };

//        [ImportingConstructor]
//        public XamlImageAssetUsageFinder()
//        {
//            this.xamlFeatureContextService = xamlFeatureContextService;
//        }

//        public IEnumerable<IImageAssetUsage> FindXamlUsages(IProjectFile projectFile,
//                                                            IImageAsset imageAsset,
//                                                            IProgressMonitor progressMonitor)
//        {
//            var context = XamlFeatureContextService.CreateXamlFeatureContext(projectFile.CompilationProject, projectFile.FilePath, 0);

//            if (context == null)
//            {
//                return Enumerable.Empty<IImageAssetUsage>();
//            }

//            return FindUsages(context.SyntaxTree.Root, imageAsset, context);
//        }

//        IEnumerable<IImageAssetUsage> FindUsages(XmlNode syntax, IImageAsset imageAsset, IXamlFeatureContext context)
//        {
//            var usages = new List<IImageAssetUsage>();

//            var elementType = context.XamlSemanticModel.GetSymbol(syntax);

//            // Search for elements that

//            // Usage kinds:
//            //  - ImageSource properties
//            //  - Setter's where the property is an image source.
//            //  - OnPlatforms
//            //  - OnIdiom.

//            return usages;
//        }

//        public bool IsAvailable(IProjectFile projectFile)
//        {
//            return projectFile.CompilationProject.HasAssemblyReference("Xamarin.Forms.Core");
//        }

//        public IEnumerable<IImageAssetUsage> FindUsages(IImageAsset imageAsset, IProjectFile projectFile, ITextProvider textProvider, IProgressMonitor progressMonitor)
//        {
//            return FindXamlUsages(pfi)
//        }

//        public Task<IEnumerable<IImageAssetUsage>> FindUsagesAsync(IImageAsset imageAsset, IProjectFile projectFile, ITextProvider textProvider, IProgressMonitor progressMonitor)
//        {
//            return Task.Run(() => FindUsages(imageAsset, projectFile, textProvider, progressMonitor));
//        }
//    }
//}
