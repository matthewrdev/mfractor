using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code;
using MFractor.Fonts;
using MFractor.Fonts.WorkUnits;
using MFractor.Ide.WorkUnits;
using MFractor.Images;
using MFractor.Images.WorkUnits;
using MFractor.Localisation;
using MFractor.Maui.StaticResources;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Navigation;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Repositories;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Navigation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class XamlNavigationHandler : NavigationHandler
    {
        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        IXamlSymbolResolver SymbolResolver => symbolResolver.Value;

        readonly Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine;
        protected IResourcesDatabaseEngine ResourcesDatabaseEngine => resourcesDatabaseEngine.Value;

        readonly Lazy<IDynamicResourceResolver> dynamicResourceResolver;
        IDynamicResourceResolver DynamicResourceResolver => dynamicResourceResolver.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<IFeatureContextFactoryRepository> featureContextFactories;
        public IFeatureContextFactoryRepository FeatureContextFactories => featureContextFactories.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public XamlNavigationHandler(Lazy<IXamlSymbolResolver> symbolResolver,
                                     Lazy<IWorkEngine> workEngine,
                                     Lazy<IProjectService> projectService,
                                     Lazy<IFeatureContextFactoryRepository> featureContextFactories,
                                     Lazy<IProductInformation> productInformation,
                                     Lazy<IResourcesDatabaseEngine> resourcesDatabaseEngine,
                                     Lazy<IDynamicResourceResolver> dynamicResourceResolver,
                                     Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.symbolResolver = symbolResolver;
            this.workEngine = workEngine;
            this.projectService = projectService;
            this.featureContextFactories = featureContextFactories;
            this.productInformation = productInformation;
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
            this.dynamicResourceResolver = dynamicResourceResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public override Task<IReadOnlyList<IWorkUnit>> Navigate(INavigationContext navigationContext, INavigationSuggestion navigationSuggestion)
        {
            var factory = FeatureContextFactories.GetInterestedFeatureContextFactory(navigationContext.CompilationProject, navigationContext.FilePath);

            var caretOffset = navigationContext.CaretOffset;

            var context = (factory.Retrieve(navigationContext.FilePath) ?? factory.CreateFeatureContext(navigationContext.CompilationProject, navigationContext.FilePath, caretOffset)) as IXamlFeatureContext;

            var symbolInfo = SymbolResolver.Resolve(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, caretOffset);

            var result = Navigate(context, symbolInfo);

            return Task.FromResult(result);
        }

        IReadOnlyList<IWorkUnit> Navigate(IXamlFeatureContext context, XamlSymbolInfo symbolInfo)
        {
            switch (symbolInfo.SymbolKind)
            {
                case XamlSymbolKind.Symbol:
                    return GoToXamlSymbol(symbolInfo, context.XamlDocument, context);
                case XamlSymbolKind.Syntax:
                    break;
                case XamlSymbolKind.Expression:
                    break;
                case XamlSymbolKind.Font:
                    return GoToFontResource(symbolInfo, context);
                case XamlSymbolKind.StaticResource:
                    return GoToStaticResource(symbolInfo, context);
                case XamlSymbolKind.DynamicResource:
                    return GoToDynamicResource(symbolInfo, context);
                case XamlSymbolKind.Image:
                    return new ViewImageAssetWorkUnit(symbolInfo.GetSymbol<IImageAsset>()).AsList();
                case XamlSymbolKind.Color:
                    break;
                case XamlSymbolKind.Localisation:
                    return GoToLocalisation(symbolInfo, context);
                case XamlSymbolKind.AutomationId:
                    break;
            }

            return Array.Empty<IWorkUnit>();
        }

        IReadOnlyList<IWorkUnit> GoToFontResource(XamlSymbolInfo symbolInfo, IXamlFeatureContext context)
        {
            return new FontViewerWorkUnit()
            {
                Font = symbolInfo.Symbol as IFont
            }.AsList();
        }

        IReadOnlyList<IWorkUnit> GoToLocalisation(XamlSymbolInfo symbolInfo, IXamlFeatureContext context)
        {
            var localisations = symbolInfo.Symbol as ILocalisationDeclarationCollection;
            if (localisations is null)
            {
                return Array.Empty<IWorkUnit>();
            }

            var navigations = new List<NavigateToFileSpanWorkUnit>();

            foreach (var localisation in localisations)
            {
                navigations.Add(new NavigateToFileSpanWorkUnit(localisation.KeySpan, localisation.ProjectFile.FilePath));
            }

            return new NavigateToFileSpansWorkUnit(navigations).AsList();
        }

        IReadOnlyList<IWorkUnit> GoToDynamicResource(XamlSymbolInfo symbolInfo, IXamlFeatureContext context)
        {
            var result = symbolInfo.AdditionalData as DynamicResourceResult;

            if (result == null || result.Definition == null)
            {
                return null;
            }

            var definition = result.Definition;
            var resources = DynamicResourceResolver.FindAvailableNamedDynamicResources(context.Project, context.Platform, context.XamlDocument.FilePath, definition.Name);

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            var workUnits = new List<NavigateToFileSpanWorkUnit>();

            foreach (var resource in resources)
            {
                var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(resource.Definition.ProjectFileKey);

                if (file != null)
                {
                    workUnits.Add(new NavigateToFileSpanWorkUnit(resource.Definition.ExpressionSpan, file.FilePath, resource.Project, true));
                }
            }

            return new NavigateToFileSpansWorkUnit(workUnits).AsList();
        }

        IReadOnlyList<IWorkUnit> GoToStaticResource(XamlSymbolInfo symbolInfo, IXamlFeatureContext context)
        {
            var result = symbolInfo.AdditionalData as StaticResourceResult;

            if (result == null || result.Definition == null)
            {
                return null;
            }

            var definition = result.Definition;

            var database = ResourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);
            var file = database.GetRepository<ProjectFileRepository>().GetProjectFileById(definition.ProjectFileKey);

            if (file == null)
            {
                return null;
            }

            return new NavigateToFileSpanWorkUnit(definition.NameSpan, file.FilePath, result.Project, true).AsList();
        }

        IReadOnlyList<IWorkUnit> GoToXamlSymbol(XamlSymbolInfo symbolInfo,
                                              IParsedXamlDocument document,
                                              IXamlFeatureContext context)
        {
            var symbol = symbolInfo.GetSymbol<ISymbol>();

            var xamlFilePath = "";
            var project = SymbolHelper.GetProjectForSymbol(context.Solution, symbol);
            if (symbol.DeclaringSyntaxReferences.Any())
            {
                var declaringSyntax = symbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax();

                if (declaringSyntax != null)
                {
                    var filePath = declaringSyntax.SyntaxTree.FilePath;
                    filePath = filePath.Substring(0, filePath.Length - ".cs".Length);

                    var files = ProjectService.GetProjectFiles(project);

                    if (files.Any(ad => ad.FilePath == filePath)
                        && filePath != document.FilePath)
                    {
                        xamlFilePath = filePath;
                    }
                }
            }

            if (!string.IsNullOrEmpty(xamlFilePath) && project != null)
            {
                return new OpenFileWorkUnit(xamlFilePath, project).AsList();
            }
            else
            {
                return new NavigateToSymbolWorkUnit(symbol, false).AsList();
            }
        }

        public override Task<INavigationSuggestion> Suggest(INavigationContext navigationContext)
        {
            if (navigationContext == null)
            {
                return Task.FromResult< INavigationSuggestion>(default);
            }

            var factory = FeatureContextFactories.GetInterestedFeatureContextFactory(navigationContext.CompilationProject, navigationContext.FilePath);

            if (factory == null)
            {
                return Task.FromResult<INavigationSuggestion>(default);
            }

            var caretOffset = navigationContext.CaretOffset;

            var context = (factory.Retrieve(navigationContext.FilePath) ?? factory.CreateFeatureContext(navigationContext.CompilationProject, navigationContext.FilePath, caretOffset)) as IXamlFeatureContext;
            if (context == null)
            {
                return Task.FromResult<INavigationSuggestion>(default);
            }

            var resolveResult = SymbolResolver.Resolve(context.XamlDocument, context.XamlSemanticModel, context.Platform, context.Project, context.Compilation, context.Namespaces, caretOffset);

            if (resolveResult == null)
            {
                return Task.FromResult<INavigationSuggestion>(default);
            }

            // Font Navigation unsupported in Windows/Mac until font viewers are rebuilt in native. 
            if (resolveResult.SymbolKind == XamlSymbolKind.Font)
            {
                return Task.FromResult<INavigationSuggestion>(default);
            }

            var symbolKind = resolveResult.SymbolKind.ToString().SeparateUpperLettersBySpace();
            if (resolveResult.Symbol == null)
            {
                var isDynamicResource = resolveResult.SymbolKind == XamlSymbolKind.DynamicResource && resolveResult.AdditionalData is DynamicResourceResult;

                if (!isDynamicResource)
                {
                    return Task.FromResult<INavigationSuggestion>(default);
                }
            }
            else if (resolveResult.SymbolKind == XamlSymbolKind.Symbol && resolveResult.Expression is BindingExpression)
            {
                symbolKind = "Binding Context Property";
            }

            var label = $"Navigate To {symbolKind}";
            var description = "";

            return Task.FromResult<INavigationSuggestion>(CreateSuggestion(label, description));
        }

        public override bool IsAvailable(INavigationContext navigationContext)
        {
            if (Path.GetExtension(navigationContext.FilePath) != ".xaml")
            {
                return false;
            }

            return XamlPlatforms.CanResolvePlatform(navigationContext.CompilationProject);
        }
    }
}