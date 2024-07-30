//using System;
//using System.Composition;
//using System.Linq;
//using System.Threading.Tasks;
//using MFractor.Analytics;
//using MFractor.Code.CodeActions;
//using MFractor.Maui;
//using MFractor.Licensing;
//using MFractor.Work;
//using MFractor.Workspace;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CodeRefactorings;

// TODO: Fix me!

//namespace MFractor.Editor.XAML.CodeActions
//{
//    [ExportCodeRefactoringProvider("Xaml", Name = "MFractor.Xaml.Refactorings"), Shared]
//    sealed class XamlRefactoringProvider : CodeRefactoringProvider
//    {
//        readonly Logging.ILogger log = Logging.Logger.Create();

//        [ImportingConstructor]
//        public XamlRefactoringProvider(Lazy<IProjectService> projectService,
//                                       Lazy<ICodeActionEngine> codeActionEngine,
//                                       Lazy<ILicensingService> licensingService,
//                                       Lazy<ILicenseUsageValidator> licenseUsageValidator,
//                                       Lazy<IWorkEngine> workEngine,
//                                       Lazy<IXamlFeatureContextService> xamlFeatureContextService,
//                                       Lazy<IAnalyticsService> analyticsService)
//        {
//            this.projectService = projectService;
//            this.codeActionEngine = codeActionEngine;
//            this.licensingService = licensingService;
//            this.licenseUsageValidator = licenseUsageValidator;
//            this.workEngine = workEngine;
//            this.xamlFeatureContextService = xamlFeatureContextService;
//            this.analyticsService = analyticsService;
//        }

//        readonly Lazy<IProjectService> projectService;
//        public IProjectService ProjectService => projectService.Value;

//        readonly Lazy<ICodeActionEngine> codeActionEngine;
//        public ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

//        readonly Lazy<ILicensingService> licensingService;
//        public ILicensingService LicensingService => licensingService.Value;

//        readonly Lazy<ILicenseUsageValidator> licenseUsageValidator;
//        public ILicenseUsageValidator LicenseUsageValidator => licenseUsageValidator.Value;

//        readonly Lazy<IWorkEngine> workEngine;
//        public IWorkEngine WorkEngine => workEngine.Value;

//        readonly Lazy<IXamlFeatureContextService> xamlFeatureContextService;
//        public IXamlFeatureContextService XamlFeatureContextService => xamlFeatureContextService.Value;

//        readonly Lazy<IAnalyticsService> analyticsService;
//        IAnalyticsService AnalyticsService => analyticsService.Value;

//        static readonly CodeActionCategory[] categoryFilters = {
//            CodeActionCategory.Fix,
//            CodeActionCategory.Refactor,
//            CodeActionCategory.Generate,
//            CodeActionCategory.Organise,
//            CodeActionCategory.Misc,
//        };

//        public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
//        {
//            if (!LicenseUsageValidator.TryUseFeature(context.Document.FilePath))
//            {
//                return Task.CompletedTask;
//            }

//            try
//            {
//                var featureContext = XamlFeatureContextService.Retrieve(context.Document.FilePath);

//                if (featureContext == null)
//                {
//                    return Task.CompletedTask;
//                }

//                featureContext.Syntax = XamlFeatureContextService.GetSyntaxAtLocation(featureContext.Document.AbstractSyntaxTree, context.Span.Start);

//                if (featureContext?.Syntax == null)
//                {
//                    return Task.CompletedTask;
//                }

//                var location = new InteractionLocation(context.Span.Start, context.Span);

//                var codeActions = CodeActionEngine.RetrieveCodeActions(featureContext, location, categoryFilters);

//                if (codeActions != null && codeActions.Any())
//                {
//                    foreach (var codeAction in codeActions)
//                    {
//                        var suggestions = codeAction.Suggest(featureContext, location);
//                        foreach (var suggestion in suggestions)
//                        {
//                            context.RegisterRefactoring(new WorkEngineCodeAction(featureContext, codeAction, location, suggestion, AnalyticsService, WorkEngine));
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                log?.Exception(ex);
//            }

//            return Task.CompletedTask;
//        }
//    }
//}
