using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.CodeGeneration;
using MFractor.Code.Formatting;
using MFractor.Code.WorkUnits;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Utilities.SyntaxVisitors;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code.WorkUnitHandlers
{
    class InsertSyntaxNodeWorkUnitHandler : WorkUnitHandler<InsertSyntaxNodesWorkUnit>
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<ICSharpSyntaxReducer> reducer;
        protected ICSharpSyntaxReducer Reducer => reducer.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        [ImportingConstructor]
        public InsertSyntaxNodeWorkUnitHandler(Lazy<IWorkspaceService> workspaceService,
                                               Lazy<ICSharpSyntaxReducer> reducer,
                                               Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                               Lazy<IProductInformation> productInformation)
        {
            this.workspaceService = workspaceService;
            this.reducer = reducer;
            this.formattingPolicyService = formattingPolicyService;
            this.productInformation = productInformation;
        }

        readonly static SyntaxAnnotation insertedSyntaxAnnotation = new SyntaxAnnotation("INSERTION_ANNOTATAION");

        public override async Task<IWorkExecutionResult> OnExecute(InsertSyntaxNodesWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var formattingOptions = FormattingPolicyService.GetFormattingPolicy(workUnit.Project);

            var workspace = workUnit.Workspace ?? WorkspaceService.CurrentWorkspace;
            var location = workUnit.HostNode.GetLocation();

            var compilationProject = workUnit.Project;
            var compilation = await compilationProject.GetCompilationAsync();

            var document = compilationProject.Documents.FirstOrDefault(d => d.FilePath == workUnit.HostNode.SyntaxTree.FilePath);

            var root = await document.GetSyntaxRootAsync();
            var model = compilation.GetSemanticModel(root.SyntaxTree);

            var sourceNode = root.FindNode(location.SourceSpan);

            var targetNode = default(SyntaxNode);

            var incomingNodes = new List<SyntaxNode>();
            var additionalUsings = new List<UsingDirectiveSyntax>();

            foreach (var syntaxNode in workUnit.SyntaxNodes)
            {
                var node = syntaxNode;

                node = Formatter.Format(syntaxNode, workspace, formattingOptions.OptionSet);

                node = Reducer.Reduce(node, model, new List<string>(), ref additionalUsings);

                incomingNodes.Add(node.WithAdditionalAnnotations(Simplifier.Annotation, Formatter.Annotation, insertedSyntaxAnnotation));
            }

            if (sourceNode is ClassDeclarationSyntax classDeclaration)
            {
                var members = incomingNodes.OfType<MemberDeclarationSyntax>().ToList();
                targetNode = InsertIntoClassDeclaration(members, classDeclaration, workUnit.AnchorNode, workUnit.InsertionLocation);
            }
            else if (sourceNode is MethodDeclarationSyntax method)
            {
                if (workUnit.AnchorNode != null || workUnit.InsertionLocation != InsertionLocation.Default)
                {
                    log?.Warning("MethodDeclarationSyntax hosts do not support anchors.");
                }
                targetNode = method.AddBodyStatements(incomingNodes.OfType<StatementSyntax>().ToArray());
            }
            else if (sourceNode is BlockSyntax block)
            {
                if (workUnit.AnchorNode != null || workUnit.InsertionLocation != InsertionLocation.Default)
                {
                    log?.Warning("BlockSyntax hosts do not support anchors.");
                }

                targetNode = block.AddStatements(incomingNodes.OfType<StatementSyntax>().ToArray());
            }

            var newRoot = root.ReplaceNode(sourceNode, targetNode);

            document = document.WithSyntaxRoot(newRoot);

            document = await Formatter.FormatAsync(document, Formatter.Annotation, formattingOptions.OptionSet).ConfigureAwait(false);

            document = await Simplifier.ReduceAsync(document, Simplifier.Annotation, formattingOptions.OptionSet).ConfigureAwait(false);

            root = await document.GetSyntaxRootAsync().ConfigureAwait(false);

            var annotatedNodes = root.GetAnnotatedNodes(insertedSyntaxAnnotation).ToList();

            var insertions = InsertSyntax(document, root, annotatedNodes, additionalUsings);

            var result = new WorkExecutionResult();
            if (insertions != null && insertions.Any())
            {
                result.AddPostProcessedWorkUnits(insertions);

                if (ProductInformation.Product == Product.VisualStudioWindows)
                {
                    var insertion = insertions.OfType<InsertTextWorkUnit>().FirstOrDefault();
                    if (insertion != null)
                    {
                        var span = new TextSpan(insertion.Offset, 0);
                        var navigationWorkUnit = new NavigateToFileSpanWorkUnit(span, document.FilePath, compilationProject);

                        result.AddPostProcessedWorkUnit(navigationWorkUnit);
                    }
                }
            }

            return result;
        }

        SyntaxNode InsertIntoClassDeclaration(List<MemberDeclarationSyntax> incomingNodes, ClassDeclarationSyntax classDeclaration, SyntaxNode anchorNode, InsertionLocation insertionLocation)
        {
            var members = classDeclaration.Members;
            if (anchorNode == null && members.Any())
            {
                switch (insertionLocation)
                {
                    case InsertionLocation.End:
                        members = members.AddRange(incomingNodes);
                        break;
                    case InsertionLocation.Start:
                    case InsertionLocation.Default:
                        members = members.InsertRange(0, incomingNodes);
                        break;
                }

                return classDeclaration.WithMembers(members);
            }

            if (anchorNode is MemberDeclarationSyntax member && members.Contains(member))
            {
                var index = members.IndexOf(member);

                if (insertionLocation == InsertionLocation.End)
                {
                    index += 1;
                }

                if (index >= members.Count)
                {
                    members = members.AddRange(incomingNodes.ToArray());
                }
                else
                {
                    members = members.InsertRange(index, incomingNodes);
                }

                return classDeclaration.WithMembers(members);
            }

            return classDeclaration.AddMembers(incomingNodes.ToArray());
        }

        List<UsingDirectiveSyntax> RemoveDuplicateUsings(SyntaxNode root,
                                                         List<UsingDirectiveSyntax> additionalUsings,
                                                         out int insertionLocation)
        {
            insertionLocation = 0;
            var usingsVisitor = new UsingDirectiveVisitor();
            usingsVisitor.Visit(root);

            var usings = new Dictionary<string, UsingDirectiveSyntax>();

            foreach (var use in additionalUsings)
            {
                if (!usings.ContainsKey(use.ToString()))
                {
                    usings[use.ToString()] = use;
                }
            }

            foreach (var use in usingsVisitor.Usings)
            {
                if (usings.ContainsKey(use.ToString()))
                {
                    usings.Remove(use.ToString());
                }

                if (use.Span.End > insertionLocation)
                {
                    insertionLocation = use.Span.End;
                }
            }

            return usings.Values.ToList();
        }

        List<IWorkUnit> InsertSyntax(Document document, SyntaxNode host, List<SyntaxNode> annotatedNodes, List<UsingDirectiveSyntax> additionalUsings)
        {
            var insertion = string.Empty;

            var result = new List<IWorkUnit>();
            var normalisedRoot = host.NormalizeWhitespace();
            var normalisedAnnotatedNodes = normalisedRoot.GetAnnotatedNodes(insertedSyntaxAnnotation).ToList();
            for (var i = 0; i < annotatedNodes.Count(); ++i)
            {
                var annotated = annotatedNodes[i];
                var normalised = normalisedAnnotatedNodes[i];

                insertion += normalised.GetLeadingTrivia();
                insertion += annotated.ToString();
                insertion += normalised.GetTrailingTrivia();
            }

            var offset = annotatedNodes.OrderBy(an => an.FullSpan.Start).First().FullSpan.Start;

            result.Add(new InsertTextWorkUnit(insertion, offset, document.FilePath));

            if (additionalUsings != null && additionalUsings.Any())
            {
                var usings = RemoveDuplicateUsings(host, additionalUsings, out var insertionLocation);

                if (usings != null && usings.Any())
                {
                    var usingsCode = Environment.NewLine + string.Join(Environment.NewLine, usings.Select(use => use.ToString()));

                    result.Add(new InsertTextWorkUnit(usingsCode, insertionLocation, document.FilePath));
                }
            }

            return result;
        }
    }
}
