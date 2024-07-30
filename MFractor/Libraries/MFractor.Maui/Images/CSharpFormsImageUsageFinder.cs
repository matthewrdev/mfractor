//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using MFractor.Images;
//using MFractor.Progress;
//using MFractor.Text;

//namespace MFractor.Maui.Images
//{
//    class CSharpFormsImageUsageFinder : IImageAssetUsageFinder
//    {
//        public string[] SupportedFileExtensions { get; } = new string[] { ".cs" };


//        readonly Lazy<IXamlFeatureContextService> xamlFeatureContextService;
//        public IXamlFeatureContextService XamlFeatureContextService => xamlFeatureContextService.Value;

//        readonly Lazy<IProjectService> projectService;
//        public IProjectService ProjectService => projectService.Value;

//        readonly Lazy<ITextProviderService> textProviderService;
//        public ITextProviderService TextProviderService => textProviderService.Value;

//        public string[] SupportedFileExtensions { get; } = new string[] { ".xaml" };

//        [ImportingConstructor]
//        public CSharpFormsImageUsageFinder(Lazy<IXamlFeatureContextService> xamlFeatureContextService,
//                                           Lazy<IProjectService> projectService,
//                                           Lazy<ITextProviderService> textProviderService)
//        {
//            this.xamlFeatureContextService = xamlFeatureContextService;
//            this.projectService = projectService;
//            this.textProviderService = textProviderService;
//        }

//        public IEnumerable<IImageAssetUsage> FindUsages(ProjectIdentifier projectIdentifier, IImageAsset imageAsset, IProgressMonitor progressMonitor)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<IEnumerable<IImageAssetUsage>> FindUsagesAsync(ProjectIdentifier projectIdentifier, IImageAsset imageAsset, IProgressMonitor progressMonitor)
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsAvailable(ProjectIdentifier projectIdentifier)
//        {
//            var project = ProjectService.GetProject(projectIdentifier);

//            if (project != null)
//            {
//                return false;
//            }

//            return project.HasAssemblyReference("Xamarin.Forms.Core");
//        }


//        public IEnumerable<IImageAssetUsage> FindCSharpUsages(IProjectFile projectFile,
//                                                              IImageAsset imageAsset,
//                                                              IProgressMonitor progressMonitor)
//        {
//            var document = projectFile.CompilationDocument;

//            if (document is null)
//            {
//                return Enumerable.Empty<IImageAssetUsage>();
//            }

//            if (!document.TryGetSyntaxRoot(out var syntaxRoot))
//            {
//                return Enumerable.Empty<IImageAssetUsage>();
//            }

//            var walker = new StringSyntaxWalker();

//            walker.Visit(syntaxRoot);

//            foreach (var @string in walker.LiteralExpressionSyntax)
//            {

//            }


//            var context = XamlFeatureContextService.CreateXamlFeatureContext(projectFile.CompilationProject, projectFile.FilePath, 0);

//            if (context == null)
//            {
//                return Enumerable.Empty<IImageAssetUsage>();
//            }

//            return FindUsages(context.SyntaxTree.Root, imageAsset, context);
//        }
//    }
//}