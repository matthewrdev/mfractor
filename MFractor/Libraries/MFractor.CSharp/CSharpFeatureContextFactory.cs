using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Code.CodeActions;
using MFractor.Configuration;
using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MFractor.Workspace;
using MFractor.Code;
using MFractor.Workspace.Utilities;

namespace MFractor.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CSharpFeatureContextFactory : IFeatureContextFactory
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkspaceService> workspaceService;
        public IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public CodeActionExecutionType[] SupportedExecutionTypes { get; } = { CodeActionExecutionType.ContextMenuCommand, };

        [ImportingConstructor]
        public CSharpFeatureContextFactory(Lazy<IWorkspaceService> workspaceService,
                                           Lazy<IProjectService> projectService)
        {
            this.workspaceService = workspaceService;
            this.projectService = projectService;
        }

        readonly object documentCacheLock = new object();
        readonly Dictionary<string, IFeatureContext> documentCache = new Dictionary<string, IFeatureContext>();

        public IFeatureContext Retrieve(string fileName)
        {
            IFeatureContext context = null;

            if (documentCache.ContainsKey(fileName))
            {
                context = documentCache[fileName];
            }

            return context;
        }

        public void Store(IFeatureContext context, string fileName)
        {
            lock (documentCacheLock)
            {
                documentCache[fileName] = context;
            }
        }

        public Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int interactionOffset, CancellationToken token)
        {
            var result = CreateFeatureContext(project, filePath, interactionOffset);

            return Task.FromResult(result);
        }

        public IFeatureContext CreateFeatureContext(Project project, string filePath, int offset)
        {
            var analysisDocument = project.Documents.FirstOrDefault(d => d.FilePath == filePath);
            var syntaxTree = SyntaxTreeForDocument(analysisDocument);

            return CreateFeatureContext(project, filePath, syntaxTree, offset);
        }

        public object GetSyntaxAtLocation(object syntaxTree, int offset)
        {
            var ast = syntaxTree as SyntaxTree;
            if (ast == null)
            {
                return null;
            }

            var syntaxRoot = ast.GetRoot();

            var span = TextSpan.FromBounds(offset, offset + 1);

            SyntaxNode node = null;
            try
            {
                if (syntaxRoot.Span.IntersectsWith(span) && span.Start >= syntaxRoot.SpanStart && span.End <= syntaxRoot.Span.End)
                {
                    node = syntaxRoot.FindNode(span);
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                log?.Info(aex.ToString());
            }
            catch (Exception ex)
            {
                log?.Warning(ex.ToString());
            }

            return node;
        }

        public bool IsInterestedInDocument(Project project, string filePath)
        {
            return SyntaxTreeForDocument(project, filePath) != null;
        }

        public SyntaxTree SyntaxTreeForDocument(Document analysisDocument)
        {
            if (analysisDocument == null)
            {
                return null;
            }

            if (!analysisDocument.TryGetSyntaxTree(out var syntaxTree))
            {
                return null;
            }

            return syntaxTree;
        }

        public SyntaxTree SyntaxTreeForDocument(Project project, string filePath)
        {
            if (project == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (!Path.GetExtension(filePath).Equals(".cs", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var analysisDocument = project.Documents.FirstOrDefault(d => d.FilePath == filePath);

            return SyntaxTreeForDocument(analysisDocument);
        }

        public IFeatureContext CreateFeatureContext(Project project, string filePath, object syntaxTree, int interactionOffset)
        {
            var ast = syntaxTree as SyntaxTree;
            if (ast == null)
            {
                return default;
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return default;
            }

            var syntax = GetSyntaxAtLocation(syntaxTree, interactionOffset);

            var syntaxNode = syntax as SyntaxNode;

            var workspace = WorkspaceService.CurrentWorkspace;

            var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);

            var documentContext = new ParsedCSharpDocument(filePath, syntaxTree as SyntaxTree, projectFile);

            var semanticModel = compilation.GetSemanticModel(ast);

            var configId = ConfigurationId.Create(project.GetIdentifier());

            var context = new FeatureContext(workspace,
                                             project.Solution,
                                             project,
                                             documentContext,
                                             syntaxNode,
                                             semanticModel,
                                             configId);

            return context;
        }

        public Task<IFeatureContext> CreateFeatureContextAsync(Project project, string filePath, int interactionOffset, object syntaxTree, CancellationToken token)
        {
            var context = CreateFeatureContext(project, filePath, syntaxTree, interactionOffset);

            return Task.FromResult(context);
        }
    }
}
