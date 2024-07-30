using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Code.Analysis;
using MFractor.Maui.Mvvm;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IParsedXamlDocumentFactory))]
    class ParsedXamlDocumentFactory : IParsedXamlDocumentFactory
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ParsedXamlDocumentFactory(Lazy<IWorkspaceService> workspaceService,
                                         Lazy<IXmlDocumentAnalyser> xmlDocumentAnalyser,
                                         Lazy<IMvvmResolver> mvvmResolver,
                                         Lazy<IProjectService> projectService,
                                         Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                         Lazy<IXamlFeatureContextService> xamlFeatureContextService,
                                         Lazy<IXamlNamespaceParser> xamlNamespaceResolver,
                                         Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver,
                                         Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.workspaceService = workspaceService;
            this.xmlDocumentAnalyser = xmlDocumentAnalyser;
            this.mvvmResolver = mvvmResolver;
            this.projectService = projectService;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.xamlFeatureContextService = xamlFeatureContextService;
            this.xamlNamespaceResolver = xamlNamespaceResolver;
            this.xmlnsDefinitionResolver = xmlnsDefinitionResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IXmlDocumentAnalyser> xmlDocumentAnalyser;
        public IXmlDocumentAnalyser XmlDocumentAnalyser => xmlDocumentAnalyser.Value;

        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<IXamlFeatureContextService> xamlFeatureContextService;
        public IXamlFeatureContextService XamlFeatureContextService => xamlFeatureContextService.Value;

        readonly Lazy<IXamlNamespaceParser> xamlNamespaceResolver;
        public IXamlNamespaceParser XamlNamespaceResolver => xamlNamespaceResolver.Value;

        readonly Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver;
        public IXmlnsDefinitionResolver XmlnsDefinitionResolver => xmlnsDefinitionResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        public IParsedXamlDocument Create(Project project, string filePath, ITextProvider textProvider)
        {
            if (textProvider is null || string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

            return Create(project, projectFile, textProvider);
        }

        public IParsedXamlDocument Create(Project project, IProjectFile projectFile, ITextProvider textProvider)
        {
            if (textProvider is null)
            {
                return null;
            }

            var filePath = projectFile.FilePath;

            try
            {
                var text = textProvider.GetText();

                XmlSyntaxTree syntaxTree = null;
                try
                {
                    syntaxTree = XmlSyntaxParser.ParseText(text);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    // Suppress
                }

                if (syntaxTree == null)
                {
                    return null;
                }

                if (!project.TryGetCompilation(out var compilation))
                {
                    log?.Warning($"TryGetCompilation failed returning null. This may affect resource synchronization. At: {System.Environment.StackTrace}");
                    return null;
                }

                var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);
                var namespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);
                var xmlnsDefinitions = XmlnsDefinitionResolver.Resolve(project, platform);

                var codeBehindSymbol = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);
                var codeBehindSyntax = codeBehindSymbol.GetNonAutogeneratedSyntax() as ClassDeclarationSyntax;
                var bindingContext = MvvmResolver.ResolveViewModelSymbol(project, filePath, syntaxTree, compilation, platform, namespaces, xmlnsDefinitions);

                return new ParsedXamlDocument(filePath, syntaxTree, bindingContext, codeBehindSymbol, codeBehindSyntax, projectFile, namespaces, xmlnsDefinitions);

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return null;
        }

        public async Task<IParsedXamlDocument> CreateAsync(Project project, IProjectFile projectFile, ITextProvider textProvider)
        {
            if (textProvider is null)
            {
                return null;
            }

            var filePath = projectFile.FilePath;

            try
            {
                var text = textProvider.GetText();

                XmlSyntaxTree syntaxTree = null;
                try
                {
                    syntaxTree = XmlSyntaxParser.ParseText(text);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                {
                    // Suppress
                }

                if (syntaxTree == null)
                {
                    return null;
                }

                var compilation = await project.GetCompilationAsync();
                var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);
                var namespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);
                var xmlnsDefinitions = XmlnsDefinitionResolver.Resolve(project, platform);

                var codeBehindSymbol = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);
                var codeBehindSyntax = codeBehindSymbol.GetNonAutogeneratedSyntax() as ClassDeclarationSyntax;
                var bindingContext = MvvmResolver.ResolveViewModelSymbol(project, filePath, syntaxTree, compilation, platform, namespaces, xmlnsDefinitions);

                return new ParsedXamlDocument(filePath, syntaxTree, bindingContext, codeBehindSymbol, codeBehindSyntax, projectFile, namespaces, xmlnsDefinitions);

            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return null;
        }
    }

}