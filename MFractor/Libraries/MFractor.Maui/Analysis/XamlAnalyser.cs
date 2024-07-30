using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Code.Analysis;
using MFractor.Concurrency;
using MFractor.Configuration;
using MFractor.Maui.Mvvm;
using MFractor.Maui.Semantics;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.Analysis
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(IXamlAnalyser))]
    class XamlAnalyser : IXamlAnalyser
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public XamlAnalyser(Lazy<IWorkspaceService> workspaceService,
                            Lazy<IXmlDocumentAnalyser> xmlDocumentAnalyser,
                            Lazy<IMvvmResolver> mvvmResolver,
                            Lazy<IProjectService> projectService,
                            Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                            Lazy<IXamlFeatureContextService> xamlFeatureContextService,
                            Lazy<IXamlNamespaceParser> xamlNamespaceResolver,
                            Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory,
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
            this.xamlSemanticModelFactory = xamlSemanticModelFactory;
            this.xmlnsDefinitionResolver = xmlnsDefinitionResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public string AnalyticsEvent => "Xaml Analysis";

        readonly TaskedBackgroundWorkerQueue workerQueue = new TaskedBackgroundWorkerQueue();

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

        readonly Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory;
        public IXamlSemanticModelFactory XamlSemanticModelFactory => xamlSemanticModelFactory.Value;

        readonly Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver;
        public IXmlnsDefinitionResolver XmlnsDefinitionResolver => xmlnsDefinitionResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        public event EventHandler<XamlAnalysisResultEventArgs> OnAnalysisCompleted;

        public void Analyse(ITextProvider textProvider,
                            string filePath,
                            ProjectId projectId,
                            CancellationToken token)
        {
            if (textProvider is null || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            //TODO: Implement debounce of 250ms on the XAML analysis for a given file.

            workerQueue.QueueTask(async () =>
            {
#if DEBUG
                using (var profiler = Profiler.Profile(context: $"DEBUG - Xaml Analysis of {Path.GetFileName(filePath)}"))
#endif
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var text = await textProvider.GetTextAsync();

                        XmlSyntaxTree syntaxTree = null;
                        try
                        {
                            syntaxTree = XmlSyntaxParser.ParseText(text);
                        }
                        catch { } // Suppressed.

                        token.ThrowIfCancellationRequested();
                        if (syntaxTree == null)
                        {
                            return;
                        }

                        var workspace = WorkspaceService.CurrentWorkspace;

                        var project = workspace.CurrentSolution.GetProject(projectId);
                        var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

                        if (project == null)
                        {
                            log?.Info("Failed to get the compilation project for " + projectId);
                            return;
                        }

                        token.ThrowIfCancellationRequested();
                        var compilation = await project.GetCompilationAsync(token);
                        project.TryGetCompilation(out var c2);
                        if (compilation is null)
                        {
                            log?.Info("Failed to get the compilation for " + projectId);
                            return;
                        }

                        var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);
                        if (platform is null)
                        {
                            log?.Info("Failed to resolve the xaml platform for " + projectId);
                            return;
                        }

                        var configId = ConfigurationId.Create(project.GetIdentifier());

                        var namespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);

                        var xmlnsDefinitions = XmlnsDefinitionResolver.Resolve(project, platform);

                        token.ThrowIfCancellationRequested();

                        var codeBehindSymbol = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);
                        var codeBehindSyntax = codeBehindSymbol.GetNonAutogeneratedSyntax() as ClassDeclarationSyntax;
                        var bindingContext = MvvmResolver.ResolveViewModelSymbol(project, filePath, syntaxTree, compilation, platform, namespaces, xmlnsDefinitions);


                        token.ThrowIfCancellationRequested();

                        var parsedDocument = new ParsedXamlDocument(filePath, syntaxTree, bindingContext, codeBehindSymbol, codeBehindSyntax, projectFile, namespaces, xmlnsDefinitions);

                        token.ThrowIfCancellationRequested();

                        var semanticModel = XamlSemanticModelFactory.Create(parsedDocument, project, compilation, namespaces);

                        var featureContext = new XamlFeatureContext(workspace, project.Solution, project, parsedDocument, compilation, semanticModel, platform, null, configId);

                        XamlFeatureContextService.Store(featureContext, parsedDocument.FilePath);

                        var results = XmlDocumentAnalyser.Analyse(featureContext.XamlDocument, featureContext, token);

                        token.ThrowIfCancellationRequested();
                        if (results.Any())
                        {
                            OnAnalysisCompleted?.Invoke(this, new XamlAnalysisResultEventArgs(filePath, projectId, results.ToList()));
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Suppress.
#if DEBUG
                        profiler.Cancelled = true;
#endif
                    }
                    catch (OperationCanceledException)
                    {
                        // Suppress.
#if DEBUG
                        profiler.Cancelled = true;
#endif
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            });
        }

        public void Dispose()
        {
            this.workerQueue.Dispose();
        }
    }
}