using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeActions;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Configuration.Attributes;
using MFractor.CSharp;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeGeneration.Commands;
using MFractor.Maui.CodeGeneration.CSharp;
using MFractor.Maui.Mvvm;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.ViewModels;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Generate
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(GenerateNewViewModel))]
    [Obsolete("The generate view model code action is no longer visible and the core behaviour will soon be refactored into its own code generator")]
    public class GenerateNewViewModel : GenerateXamlCodeAction
    {
        const string pageSuffix = "page";
        const string viewSuffix = "view";

        [ExportProperty("The fully qualified type to use as the base class for the view model. For example, `MvvmFramework.ViewModels.BaseViewModel`.")]
        public string BaseClass { get; set; } = null;

        [ExportProperty("The folder path relative to the project root to insert the newly created view model class into. For example `ViewModels/Cells`. This argument also supports the $name$ argument to place the view model inside a folder using the name of the new view model.")]
        public string ViewModelsFolder { get; set; } = "ViewModels";

        [ExportProperty("The suffix to append to the end of the newly created view model. For example, setting this to `PageModel` would cause a page named `MainPage` to create a view model named `MainPageModel`.")]
        public string ViewModelSuffix { get; set; } = "ViewModel";

        [ExportProperty("When generating the new ViewModel, should it be placed in the same folder and namespace as the XAML view that is creating it? Setting this property to true will cause the " + nameof(ViewModelsFolder) + "property to be ignored.")]
        public bool ShouldPlaceViewModelWithView { get; set; } = false;

        [ExportProperty("Should MFractor remove the 'Page' or 'View' suffix from the view name before creating generating the new view model? Consider the view 'MainPage' with ViewModelSuffix set to 'ViewModel'; if set as true, the new view model would be named 'MainViewModel'. If false, the new view model would be named 'MainPageViewModel'")]
        public bool ShouldRemoveXamlViewSuffix { get; set; } = true;

        [Import]
        public ICommandImplementationGenerator CommandGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IViewModelPropertyGenerator PropertyGenerator { get; set; }

        readonly Lazy<IMarkupExpressionEvaluater> expressionEvaluater;
        IMarkupExpressionEvaluater ExpressionEvaluater => expressionEvaluater.Value;

        readonly Lazy<IDataBindingGatherer> dataBindingGatherer;
        public IDataBindingGatherer DataBindingGatherer => dataBindingGatherer.Value;

        [ImportingConstructor]
        public GenerateNewViewModel(Lazy<IMarkupExpressionEvaluater> expressionEvaluater,
                                    Lazy<IDataBindingGatherer> dataBindingGatherer)
        {
            this.expressionEvaluater = expressionEvaluater;
            this.dataBindingGatherer = dataBindingGatherer;
        }

        public override string Documentation => "Generates a new view model for the XAML view/";

        public override string Identifier => "com.mfractor.code_actions.xaml.implement_view_model";

        public override string Name => "Generate New View Model";

        /// <summary>
        /// The filter that this code action supports execution.
        /// </summary>
        /// <value>The scope to target.</value>
        public override MFractor.Code.DocumentExecutionFilter Filter => MFractor.Code.XmlExecutionFilters.XmlDocument;

        public override bool CanExecute(IParsedXamlDocument document,
                                        IXamlFeatureContext context,
                                        InteractionLocation location)
        {
            return false;
        }

        public override IReadOnlyList<ICodeActionSuggestion> Suggest(IParsedXamlDocument document,
                                                                  IXamlFeatureContext context, InteractionLocation location)
        {
            return CreateSuggestion("Generate a new ViewModel for XAML view", 0).AsList();
        }

        public override IReadOnlyList<IWorkUnit> Execute(IParsedXamlDocument document,
                                                       IXamlFeatureContext context,
                                                       ICodeActionSuggestion suggestion,
                                                       InteractionLocation location)
        {
            ClassDeclarationGenerator.InstancePropertyGenerator = PropertyGenerator;

            var viewModelProject = context.Project;

            var featureName = GetFeatureName(Path.GetFileName(document.FilePath));

            var folderPath = GetViewModelFolderPath(featureName, context, document);
            var viewModelName = featureName + ViewModelSuffix;

            var projects = new List<Project>() { viewModelProject, context.Project}.Distinct().ToList();

            IReadOnlyList<IWorkUnit> generateViewModelCallback(GenerateCodeFilesResult result)
            {
                var viewModelNamespace = NamespaceDeclarationGenerator.GetNamespaceFor(result.SelectedProject, result.FolderPath);

                var code = GenerateViewModelCode(document, context.Project, context.Compilation, context,  context.Namespaces, result.Name, viewModelNamespace);

                var viewModelVirtualFilePath = result.Name;

                if (!string.IsNullOrEmpty(result.FolderPath))
                {
                    viewModelVirtualFilePath = Path.Combine(result.FolderPath, result.Name);
                }

                return new CreateProjectFileWorkUnit(code, viewModelVirtualFilePath + ".cs", result.SelectedProject.GetIdentifier()).AsList();
            }

            return new GenerateCodeFilesWorkUnit(viewModelName,
                                                 viewModelProject,
                                                 projects,
                                                 folderPath,
                                                 "Generate View Model",
                                                 $"Generate a new view model implementation for {document.Name}",
                                                 string.Empty,
                                                 ProjectSelectorMode.Single,
                                                 generateViewModelCallback).AsList();
        }

        string GenerateViewModelCode(IParsedXamlDocument document, Project project, Compilation compilation, IXamlFeatureContext context, IXamlNamespaceCollection namespaces, string viewModelVirtualFilePath, string viewModelNamespace)
        {
            var options = FormattingPolicyService.GetFormattingPolicy(project);

            var bindings = DataBindingGatherer.GatherDataBindings(document.XamlSyntaxRoot, namespaces, context.XamlSemanticModel, context.Platform).DistinctBy(b => b.BindingValue.Value).ToList();

            ClassDeclarationSyntax classSyntax = null;

            var objectMetaType = compilation.GetTypeByMetadataName("System.Object");

            var members = new List<MemberDeclaration>();
            var nodes = new List<SyntaxNode>();

            BuildMembersList(context, bindings, objectMetaType, members, nodes);

            var baseClassType = ResolveBaseClassType(compilation);
            if (baseClassType != null)
            {
                classSyntax = ClassDeclarationGenerator.GenerateSyntax(viewModelVirtualFilePath, baseClassType, members).AddMembers(nodes.Select(n => n as MemberDeclarationSyntax).ToArray());
            }
            else
            {
                classSyntax = ClassDeclarationGenerator.GenerateSyntax(viewModelVirtualFilePath, BaseClass, members).AddMembers(nodes.Select(n => n as MemberDeclarationSyntax).ToArray());
            }

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceName = viewModelNamespace;

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(namespaceName).NormalizeWhitespace();
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));

            var syntax = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            syntax = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(syntax, context.Workspace, options.OptionSet);

            var code = syntax.ToString();
            return code;
        }

        void BuildMembersList(IXamlFeatureContext context, List<BindingExpression> bindings, INamedTypeSymbol objectMetaType, List<MemberDeclaration> members, List<SyntaxNode> nodes)
        {
            foreach (var binding in bindings)
            {
                ITypeSymbol targetReturnType = objectMetaType;

                var bindingName = binding.BindingValue.Value;
                if (bindingName.Contains("."))
                {
                    bindingName = bindingName.Split('.')[0];
                }
                else
                {
                    var symbol = context.XamlSemanticModel.GetSymbol(binding.ParentAttribute);

                    targetReturnType = SymbolHelper.ResolveMemberReturnType(symbol);
                }

                if (binding.Converter != null
                    && binding.Converter.AssignmentValue != null)
                {
                    targetReturnType = ValueConverterHelper.ResolveValueConverterInputType(context, binding.Converter, ExpressionEvaluater);
                }

                if (targetReturnType == null)
                {
                    targetReturnType = objectMetaType;
                }

                if (targetReturnType.ToString() == "System.Windows.Input.ICommand")
                {
                    var command = CommandGenerator.GenerateSyntax(bindingName, context.Platform.Command.MetaType);
                    nodes.AddRange(command);
                }
                else
                {
                    members.Add(new MemberDeclaration(targetReturnType, bindingName, MemberType.Property, Accessibility.Public));
                }
            }
        }

        public string GetViewModelFolderPath(string featureName, IXamlFeatureContext context, IParsedXamlDocument document)
        {
            var xamlDocument = ProjectService.GetProjectFileWithFilePath(context.Project, document.FilePath);
            if (ShouldPlaceViewModelWithView && xamlDocument != null)
            {
                var folders = xamlDocument.ProjectFolders.Any() ? string.Join(Path.DirectorySeparatorChar.ToString(), xamlDocument.ProjectFolders) : "";
                if (!string.IsNullOrEmpty(folders))
                {
                    return folders;
                }
            }
            else if (!string.IsNullOrEmpty(ViewModelsFolder))
            {
                return ViewModelsFolder.Replace("$name$", featureName);
            }

            return string.Empty;
        }

        public INamedTypeSymbol ResolveBaseClassType(Compilation compilation)
        {
            INamedTypeSymbol baseType = null;

            if (!string.IsNullOrEmpty(BaseClass))
            {
                baseType = compilation.GetTypeByMetadataName(BaseClass);
            }

            return baseType;
        }

        public string CreateNewClassName(string name)
        {
            return GetFeatureName(name) + ViewModelSuffix;
        }

        public string GetFeatureName(string viewFilePath)
        {
            var className = Path.GetFileNameWithoutExtension(viewFilePath);

            if (ShouldRemoveXamlViewSuffix)
            {
                if (className.EndsWith(pageSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    className = className.Remove(className.Length - pageSuffix.Length, pageSuffix.Length);
                }
                else if (className.EndsWith(viewSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    className = className.Remove(className.Length - viewSuffix.Length, viewSuffix.Length);
                }
            }

            return className;
        }
    }
}
