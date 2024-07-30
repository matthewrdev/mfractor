using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Formatting;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.Mvvm.BindingContextConnectors
{
    class CodeBehindBindingContextConnector : BindingContextConnector
    {
        const string viewModelTypeArgument = "viewModelType";
        const string viewModelNameArgument = "viewModelName";

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        [CodeSnippetArgument(viewModelTypeArgument, "The fully qualified metatype of the ViewModel.")]
        [CodeSnippetArgument(viewModelNameArgument, "The name of the ViewModel for the View.")]
        [ExportProperty("The code snippet that gets the ViewModel from the View.")]
        [CodeSnippetDefaultValue("public $viewModelType$ ViewModel => BindingContext as $viewModelType$;\n", "Gets the ViewModel for the View.")]
        public ICodeSnippet ViewModelPropertySnippet
        {
            get; set;
        }

        [CodeSnippetArgument(viewModelTypeArgument, "The fully qualified metatype of the ViewModel.")]
        [CodeSnippetArgument(viewModelNameArgument, "The name of the ViewModel for the View.")]
        [ExportProperty("The code snippet that initialises the ViewModel and assigns it to the Views binding context.")]
        [CodeSnippetDefaultValue("BindingContext = new $viewModelType$();", "Initialises the binding context for the View.")]
        public ICodeSnippet BindingContextInitialiserSnippet
        {
            get; set;
        }

        public override string Identifier => "com.mfractor.binding_context_connector.code_behind";

        public override string Name => "Initialise BindingContext In Code-Behind";

        public override string Documentation => "Initialise the View's binding context using the ViewModel within the InitialiseComponent method in the Views Code-Behind class.";

        [ImportingConstructor]
        public CodeBehindBindingContextConnector(Lazy<IWorkspaceService> workspaceService,
                                                 Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                                 Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService,
                                                 Lazy<IProjectService> projectService)
            : base(formattingPolicyService, xmlFormattingPolicyService, projectService)
        {
            this.workspaceService = workspaceService;
        }

        public override IReadOnlyList<IWorkUnit> Connect(CreateProjectFileWorkUnit view, CreateProjectFileWorkUnit codeBehind, CreateProjectFileWorkUnit viewModel, string viewModelMetaType, string viewMetaType, ProjectIdentifier projectIdentifier)
        {
            var viewModelSymbolParts = viewModelMetaType.Split('.');
            var project = ProjectService.GetProject(projectIdentifier);

            if (viewModelSymbolParts.Length > 1 || project == null)
            {
                var workspace = project.Solution.Workspace;
                var viewModelNamespace = string.Join(".", viewModelSymbolParts.Take(viewModelSymbolParts.Length - 1));

                var viewModelName = viewModelSymbolParts.Last();

                ViewModelPropertySnippet.SetArgumentValue(viewModelTypeArgument, viewModelMetaType)
                                        .SetArgumentValue(viewModelNameArgument, viewModelName);

                BindingContextInitialiserSnippet.SetArgumentValue(viewModelTypeArgument, viewModelMetaType)
                                                .SetArgumentValue(viewModelNameArgument, viewModelName);


                var syntax = SyntaxFactory.ParseCompilationUnit(codeBehind.FileContent);

                if (syntax != null)
                {
                    var ast = syntax.SyntaxTree;

                    var root = ast.GetRoot() as CompilationUnitSyntax;

                    var @namespace = root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

                    if (@namespace != null)
                    {
                        var @class = @namespace.Members.OfType<ClassDeclarationSyntax>().FirstOrDefault();

                        if (@class != null)
                        {
                            var constructors = @class.Members.OfType<ConstructorDeclarationSyntax>();

                            var constructor = constructors.FirstOrDefault(m => m.Body.Statements.OfType<ExpressionStatementSyntax>().Any(s => s.Expression is InvocationExpressionSyntax ss
                                                                                                                                                && ss.Expression is IdentifierNameSyntax ins
                                                                                                                                                && ins.Identifier.Text == "InitializeComponent"));

                            if (constructor != null)
                            {
                                var statements = constructor.Body.Statements.Insert(0, BindingContextInitialiserSnippet.AsStatement());

                                var body = constructor.Body.WithStatements(statements);

                                var newInitialiseComponent = constructor.WithBody(body);

                                var members = @class.Members.Replace(constructor, newInitialiseComponent);

                                members = members.InsertRange(0, ViewModelPropertySnippet.AsMembersList());

                                var newClass = @class.WithMembers(members);

                                var newNamespaceMembers = @namespace.Members.Replace(@class, newClass);

                                var newNamespace = @namespace.WithMembers(newNamespaceMembers);

                                var newRootMembers = root.Members.Replace(@namespace, newNamespace);

                                var newCompliationRoot = root.WithMembers(newRootMembers).NormalizeWhitespace();

                                var options = FormattingPolicyService.GetFormattingPolicy(projectIdentifier);
                                syntax = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(newCompliationRoot, workspace, options.OptionSet);

                                codeBehind.FileContent = syntax.ToString();
                            }
                        }
                    }
                }
            }

            return new List<IWorkUnit>()
            {
                view, codeBehind, viewModel,
            };
        }

        public override bool IsAvailable(ProjectIdentifier projectIdentifier)
        {
            return true;
        }
    }
}
