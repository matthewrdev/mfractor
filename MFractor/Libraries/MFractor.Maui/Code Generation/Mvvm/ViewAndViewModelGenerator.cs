using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.CSharp;
using MFractor.CSharp.CodeGeneration;
using MFractor.Maui.CodeActions.Generate;
using MFractor.Maui.CodeGeneration.Views;
using MFractor.Maui.Mvvm;
using MFractor.Maui.Mvvm.BindingContextConnectors;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Workspace.Utilities;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IViewViewModelGenerator))]
    class ViewAndViewModelGenerator : CodeGenerator, IViewViewModelGenerator
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IBindingContextConnectorService> bindingContextConnectorService;
        IBindingContextConnectorService BindingContextConnectorService => bindingContextConnectorService.Value;

        public override string[] Languages { get; } = new string[] { "XAML", "C#" };

        public override string Identifier => "com.mfractor.code_gen.xaml.view_and_view_model";

        public override string Name => "Generate Page And View Model";

        public override string Documentation => "Generates a new ContentPage with an associated view model.";

        [ExportProperty("What is the suffix to use when creating a new page?")]
        public string ViewSuffix
        {
            get;
            set;
        } = "Page";

        [ExportProperty("Where should new views be placed?")]
        public string ViewsFolder
        {
            get;
            set;
        } = "Pages";

        [ExportProperty("When the ViewBaseClass is not a platform view class, what is the XMLNS prefix to use?")]
        public string ViewXmlnsPrefix
        {
            get;
            set;
        } = "pages";

        [ExportProperty("Where should new views be placed?")]
        public string DefaultFolderLocation
        {
            get;
            set;
        } = "";

        [ExportProperty("What is the default base name to provide to the MVVM wizard?")]
        public string DefaultBaseName
        {
            get;
            set;
        } = "";

        [ExportProperty("What is the id of the binding context connector that should be used to connect the view model to the XAML view?")]
        public string DefaultBindingContextConnectorId
        {
            get;
            set;
        } = DefaultBindingContextConnector.Id;

        [Import]
        public IXamlViewWithCodeBehindGenerator XamlViewWithCodeBehindGenerator { get; set; }

        [Import]
        public GenerateNewViewModel GenerateNewViewModel { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [ImportingConstructor]
        public ViewAndViewModelGenerator(Lazy<IBindingContextConnectorService> bindingContextConnectorService)
        {
            this.bindingContextConnectorService = bindingContextConnectorService;
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, IXamlPlatform platform, ViewViewModelGenerationOptions options)
        {
            return Generate(project.GetIdentifier(), project.Solution.Workspace, platform, options);
        }

        public IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, IXamlPlatform platform, ViewViewModelGenerationOptions options)
        {
            var viewWorkUnits = GenerateView(projectIdentifier, workspace, platform, options);

            var viewModelWorkUnit = GenerateViewModel(projectIdentifier, workspace, options);

            var workUnits = new List<IWorkUnit>();
            workUnits.AddRange(viewWorkUnits);
            workUnits.Add(viewModelWorkUnit);

            if (!string.IsNullOrEmpty(options.BindingContextConnectorId))
            {
                var connector = BindingContextConnectorService.ResolveById(options.BindingContextConnectorId);

                if (connector != null)
                {
                    var viewWorkUnit = viewWorkUnits.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(r => r.FilePath.EndsWith(".xaml", System.StringComparison.OrdinalIgnoreCase));
                    var codeBehindWorkUnit = viewWorkUnits.OfType<CreateProjectFileWorkUnit>().FirstOrDefault(r => r.FilePath.EndsWith(".xaml.cs", System.StringComparison.OrdinalIgnoreCase));

                    try
                    {
                        connector.ApplyConfiguration(ConfigurationId.Create(projectIdentifier.Guid, projectIdentifier.Name));
                        workUnits = connector.Connect(viewWorkUnit, codeBehindWorkUnit, viewModelWorkUnit as CreateProjectFileWorkUnit, options.ViewModelMetadataName, options.ViewMetadataName, projectIdentifier).ToList();
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            }

            return workUnits;
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, IXamlPlatform platform, string virtualFolderPath, string baseName)
        {
            return Generate(project.GetIdentifier(), project.Solution.Workspace, platform, virtualFolderPath, baseName);
        }

        public IReadOnlyList<IWorkUnit> Generate(ProjectIdentifier projectIdentifier,
                                               CompilationWorkspace workspace,
                                               IXamlPlatform platform,
                                               string virtualFolderPath,
                                               string baseName)
        {
            if (!string.IsNullOrEmpty(virtualFolderPath) && virtualFolderPath.Contains("$name$"))
            {
                virtualFolderPath = virtualFolderPath.Replace("$name$", baseName);
            }

            var viewName = baseName + ViewSuffix;
            var viewFolder = virtualFolderPath;
            var viewBaseClass = platform.Page.MetaType;
            var viewNamespace = NamespaceDeclarationGenerator.GetNamespaceFor(projectIdentifier, viewFolder);
            var viewXmlnsPrefix = "";

            var viewModelName = baseName + GenerateNewViewModel.ViewModelSuffix;
            var viewModelFolder = GenerateNewViewModel.ShouldPlaceViewModelWithView ? virtualFolderPath : GenerateNewViewModel.ViewModelsFolder;
            var viewModelBaseClass = GenerateNewViewModel.BaseClass;
            var viewModelNamespace = NamespaceDeclarationGenerator.GetNamespaceFor(projectIdentifier, viewModelFolder);

            var options = new ViewViewModelGenerationOptions(viewName,
                                                             viewFolder,
                                                             viewBaseClass,
                                                             viewNamespace,
                                                             viewXmlnsPrefix,
                                                             projectIdentifier,
                                                             viewModelName,
                                                             viewModelFolder,
                                                             viewModelBaseClass,
                                                             viewModelNamespace,
                                                             projectIdentifier,
                                                             DefaultBindingContextConnectorId);

            return Generate(projectIdentifier, workspace, platform, options);
        }

        public IReadOnlyList<IWorkUnit> GenerateView(Project project, IXamlPlatform platform, string viewName, string viewFolderPath, string viewNamespace, string viewBaseClass, string xmlnsPrefix)
        {
            var projectIdentifier = project.GetIdentifier();

            return GenerateView(projectIdentifier, project.Solution.Workspace, platform, viewName, viewFolderPath, viewNamespace, viewBaseClass, xmlnsPrefix);
        }

        public IReadOnlyList<IWorkUnit> GenerateView(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, IXamlPlatform platform, string viewName, string viewFolderPath, string viewNamespace, string viewBaseClass, string xmlnsPrefix)
        {
            return XamlViewWithCodeBehindGenerator.Generate(viewName, viewNamespace, xmlnsPrefix, projectIdentifier, workspace, platform,  viewFolderPath, viewBaseClass);
        }

        public IReadOnlyList<IWorkUnit> GenerateView(Project project, IXamlPlatform platform, ViewViewModelGenerationOptions options)
        {
            var projectIdentifier = options.ViewProjectIdentifier ?? project.GetIdentifier();

            return GenerateView(projectIdentifier, project.Solution.Workspace, platform, options.ViewName, options.ViewFolderPath, options.ViewNamespace, options.ViewBaseClass, options.ViewXmlnsPrefix);
        }

        public IReadOnlyList<IWorkUnit> GenerateView(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, IXamlPlatform platform, ViewViewModelGenerationOptions options)
        {
            return GenerateView(options.ViewProjectIdentifier ?? projectIdentifier, workspace, platform, options.ViewName, options.ViewFolderPath, options.ViewNamespace, options.ViewBaseClass, options.ViewXmlnsPrefix);
        }

        public IWorkUnit GenerateViewModel(ProjectIdentifier projectIdentifier,
                                           CompilationWorkspace workspace,
                                           string viewModelName,
                                           string viewModelFolderPath,
                                           string viewModelNamespace,
                                           string viewModelBaseClass)
        {
            var className = viewModelName;
            var project = ProjectService.GetProject(projectIdentifier);

            INamedTypeSymbol baseClassType = null;
            if (project != null && project.TryGetCompilation(out var compilation) && !string.IsNullOrEmpty(viewModelBaseClass))
            {
                baseClassType = compilation.GetTypeByMetadataName(viewModelBaseClass);
            }


            ClassDeclarationSyntax classSyntax;
            if (baseClassType != null)
            {
                classSyntax = ClassDeclarationGenerator.GenerateSyntax(className, baseClassType, Enumerable.Empty<MemberDeclaration>());
            }
            else
            {
                classSyntax = ClassDeclarationGenerator.GenerateSyntax(className, viewModelBaseClass, Enumerable.Empty<MemberDeclaration>());
            }

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var namespaceName = viewModelNamespace;

            var namespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(namespaceName).NormalizeWhitespace();
            namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));

            var syntax = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(namespaceSyntax);

            var options = FormattingPolicyService.GetFormattingPolicy(projectIdentifier);
            syntax = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(syntax, workspace, options.OptionSet);

            var code = syntax.ToString();

            className = Path.Combine(viewModelFolderPath, className);

            var workUnit = new CreateProjectFileWorkUnit
            {
                FileContent = code,
                FilePath = className + ".cs",
                TargetProjectIdentifier = projectIdentifier,
                Identifier = "ViewModel"
            };

            return workUnit;
        }

        public IWorkUnit GenerateViewModel(Project project,
                                           string viewModelName,
                                           string viewModelFolderPath,
                                           string viewModelNamespace,
                                           string viewModelBaseClass)
        {
            return GenerateViewModel(project.GetIdentifier(), project.Solution.Workspace, viewModelName, viewModelFolderPath, viewModelNamespace, viewModelBaseClass);
        }

        public IWorkUnit GenerateViewModel(Project project, ViewViewModelGenerationOptions options)
        {
            return GenerateViewModel(options.ViewModelProjectIdentifier ?? project.GetIdentifier(), project.Solution.Workspace, options.ViewModelName, options.ViewModelFolderPath, options.ViewModelNamespace, options.ViewModelBaseClass);
        }

        public IWorkUnit GenerateViewModel(ProjectIdentifier projectIdentifier, CompilationWorkspace workspace, ViewViewModelGenerationOptions options)
        {
            return GenerateViewModel(options.ViewModelProjectIdentifier ?? projectIdentifier, workspace, options.ViewModelName, options.ViewModelFolderPath, options.ViewModelNamespace, options.ViewModelBaseClass);
        }
    }
}