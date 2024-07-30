using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Code.CodeActions;
using MFractor.Configuration;
using MFractor.Maui.Mvvm;
using MFractor.Maui.Semantics;
using MFractor.Maui.Symbols;
using MFractor.Maui.Xmlns;
using MFractor.Localisation;
using MFractor.Ide.Navigation;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Code;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXamlFeatureContextService))]
    class XamlFeatureContextService : IXamlFeatureContextService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        readonly Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService;
        public IXmlSyntaxTreeService XmlSyntaxTreeService => xmlSyntaxTreeService.Value;

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXamlSymbolResolver> symbolResolver;
        public IXamlSymbolResolver XamlSymbolResolver => symbolResolver.Value;

        readonly Lazy<IXamlNamespaceParser> xamlNamespaceResolver;
        public IXamlNamespaceParser XamlNamespaceResolver => xamlNamespaceResolver.Value;

        readonly Lazy<IXmlSyntaxFinder> xmlSyntaxFinder;        public IXmlSyntaxFinder XmlSyntaxFinder => xmlSyntaxFinder.Value;

        readonly Lazy<ILocalisationResolver> localisationResolver;
        public ILocalisationResolver LocalisationResolver => localisationResolver.Value;

        readonly Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory;
        public IXamlSemanticModelFactory XamlSemanticModelFactory => xamlSemanticModelFactory.Value;

        readonly Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver;
        public IXmlnsDefinitionResolver XmlnsDefinitionResolver => xmlnsDefinitionResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        public CodeActionExecutionType[] SupportedExecutionTypes { get; } = {
            CodeActionExecutionType.ContextMenuCommand,
            CodeActionExecutionType.QuickFixMenu,
        };

        [ImportingConstructor]
        public XamlFeatureContextService(Lazy<IMvvmResolver> mvvmResolver,
                                         Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                         Lazy<IWorkspaceService> workspaceService,
                                         Lazy<IProjectService> projectService,
                                         Lazy<IXmlSyntaxFinder> xmlSyntaxFinder,
                                         Lazy<IXamlSymbolResolver> symbolResolver,
                                         Lazy<IXamlNamespaceParser> xamlNamespaceResolver,
                                         Lazy<ILocalisationResolver> localisationResolver,
                                         Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory,
                                         Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver,
                                         Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.mvvmResolver = mvvmResolver;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.workspaceService = workspaceService;
            this.projectService = projectService;
            this.xmlSyntaxFinder = xmlSyntaxFinder;
            this.symbolResolver = symbolResolver;
            this.xamlNamespaceResolver = xamlNamespaceResolver;
            this.localisationResolver = localisationResolver;
            this.xamlSemanticModelFactory = xamlSemanticModelFactory;
            this.xmlnsDefinitionResolver = xmlnsDefinitionResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        readonly object documentCacheLock = new object();
        readonly Dictionary<string, IFeatureContext> documentCache = new Dictionary<string, IFeatureContext>();

        public IFeatureContext Retrieve(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                log?.Warning("A null or empty file name was used to try get a feature context.");
                return default;
            }

            IFeatureContext context = null;

            if (documentCache.ContainsKey(fileName))
            {
                context = documentCache[fileName];
            }

            return context;
        }

        public void Store(IFeatureContext context, string fileName)
        {
            if (context == null || string.IsNullOrEmpty(fileName))
            {
                log?.Warning("No file name and/or context provided.");
                return;
            }

            lock (documentCacheLock)
            {
                documentCache[fileName] = context;
            }
        }

        public async Task<IXamlFeatureContext> CreateXamlFeatureContextAsync(Project project,
                                                                            string filePath,
                                                                            int interactionOffset,
                                                                            CancellationToken token)
        {
            var syntaxTree = XmlSyntaxTreeService.GetSyntaxTree(filePath);

            return await CreateXamlFeatureContextAsync(project, filePath, syntaxTree, interactionOffset, token);
        }

        public IXamlFeatureContext CreateXamlFeatureContext(Project project, string filePath, int offset)
        {
            return CreateXamlFeatureContextAsync(project, filePath, offset, CancellationToken.None).Result;
        }

        public IFeatureContext CreateFeatureContext(Project project, string filePath, int offset)
        {
            return CreateXamlFeatureContextAsync(project, filePath, offset, CancellationToken.None).Result;
        }

        public async Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int offset, CancellationToken token)
        {
            return await CreateXamlFeatureContextAsync(project, filePath, offset, token);
        }

        public object GetSyntaxAtLocation(object syntaxTree, int offset)
        {
            if (!(syntaxTree is XmlSyntaxTree xmlSyntaxTree))
            {
                return null;
            }

            var path = XmlSyntaxFinder.BuildXmlPathToOffset(xmlSyntaxTree, offset, out _);

            Xml.XmlSyntax xmlSyntax = null;
            if (path != null && path.Any())
            {
                xmlSyntax = path.Last();
            }

            return xmlSyntax;
        }

        public bool IsInterestedInDocument(Project project, string filePath)
        {
            return MvvmResolver.ResolveContextType(filePath) == RelationalNavigationContextType.Definition;
        }

        public IFeatureContext CreateFeatureContext(Project project, string filePath, object syntaxTree, int interactionOffset)
        {
            return CreateXamlFeatureContext(project, filePath, syntaxTree as XmlSyntaxTree, interactionOffset);
        }

        public Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int interactionOffset, object syntaxTree, CancellationToken token)
        {
            var context = CreateFeatureContext(project, filePath, syntaxTree as XmlSyntaxTree, interactionOffset);

            return Task.FromResult<IFeatureContext>(context);
        }

        public IXamlFeatureContext CreateXamlFeatureContext(Project project, string filePath, XmlSyntaxTree syntaxTree, int interactionOffset)
        {
            if (syntaxTree is null
                   || project is null)
            {
                return null;
            }

            var syntax = GetSyntaxAtLocation(syntaxTree, interactionOffset);

            var workspace = project.Solution.Workspace;

            var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);

            var configId = ConfigurationId.Create(project.GetIdentifier());

            var namespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);
            var xmlnsDefinitions = XmlnsDefinitionResolver.Resolve(project, platform);

            var codeBehindSymbol = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);

            if (codeBehindSymbol == null)
            {
                return default;
            }

            var codeBehindSyntax = codeBehindSymbol.DeclaringSyntaxReferences.GetNonAutogeneratedSyntax() as ClassDeclarationSyntax;
            var bindingContext = MvvmResolver.ResolveViewModelSymbol(project, filePath, syntaxTree, compilation, platform, namespaces, xmlnsDefinitions);


            var parsedDocument = new ParsedXamlDocument(filePath, syntaxTree, bindingContext, codeBehindSymbol, codeBehindSyntax, projectFile, namespaces, xmlnsDefinitions);

            var semanticModel = XamlSemanticModelFactory.Create(parsedDocument, project, compilation, namespaces);

            var context = new XamlFeatureContext(workspace, project.Solution, project, parsedDocument, compilation, semanticModel, platform, syntax, configId);

            return context;
        }

        public Task<IXamlFeatureContext> CreateXamlFeatureContextAsync(Project project, string filePath, XmlSyntaxTree syntaxTree, int interactionOffset, CancellationToken token)
        {
            var context = CreateXamlFeatureContext(project, filePath, syntaxTree, interactionOffset);

            return Task.FromResult(context);
        }
    }
}
