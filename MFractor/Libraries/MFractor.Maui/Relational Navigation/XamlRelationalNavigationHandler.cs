using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;
using MFractor.Ide.Navigation;
using MFractor.Maui.Mvvm;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.RelationalNavigation
{
    class XamlRelationalNavigationHandler : Configurable, IRelationalNavigationHandler
    {
        readonly Lazy<IMvvmResolver> mvvmResolver;
        public IMvvmResolver MvvmResolver => mvvmResolver.Value;

        readonly Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService;
        public IXmlSyntaxTreeService XmlSyntaxTreeService => xmlSyntaxTreeService.Value;

        readonly Lazy<IXamlNamespaceParser> xamlNamespaceResolver;
        public IXamlNamespaceParser XamlNamespaceResolver => xamlNamespaceResolver.Value;

        readonly Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver;
        public IXmlnsDefinitionResolver XmlnsDefinitionResolver => xmlnsDefinitionResolver.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        public string DefinitionDisplayName => "XAML View";

        public string DefinitionCodeBehindDisplayName => "Code Behind";

        public string ImplementationDisplayName => "ViewModel";

        public override string Identifier => "com.mfractor.relational_navigation.xaml.relational_navigation_handler";

        public override string Name => "XAML Relational Navigation Handler";

        public override string Documentation => "Provides relational navigation support for the View/ViewModel pattern in XAML";

        [ImportingConstructor]
        public XamlRelationalNavigationHandler(Lazy<IMvvmResolver> mvvmResolver,
                                            Lazy<IXmlSyntaxTreeService> xmlSyntaxTreeService,
                                            Lazy<IXamlNamespaceParser> xamlNamespaceResolver,
                                            Lazy<IXmlnsDefinitionResolver> xmlnsDefinitionResolver,
                                            Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.mvvmResolver = mvvmResolver;
            this.xmlSyntaxTreeService = xmlSyntaxTreeService;
            this.xamlNamespaceResolver = xamlNamespaceResolver;
            this.xmlnsDefinitionResolver = xmlnsDefinitionResolver;
            this.xamlPlatforms = xamlPlatforms;
        }

        public bool IsInterestedInProject(Project project)
        {
            return XamlPlatforms.CanResolvePlatform(project);
        }

        public bool IsAvailable(Project project, string filePath)
        {
            if (!IsInterestedInProject(project))
            {
                return false;
            }

            return MvvmResolver.ResolveContextType(filePath) != RelationalNavigationContextType.Unknown;
        }

        public RelationalNavigationContextType ResolveRelationalNavigationContextType(IProjectFile projectFile)
        {
            return MvvmResolver.ResolveContextType(projectFile.FilePath);
        }

        INamedTypeSymbol GetViewModel(Project project, string filePath)
        {
            var syntaxTree = XmlSyntaxTreeService.GetSyntaxTree(filePath);

            INamedTypeSymbol viewModel = null;
            if (syntaxTree != null
                && project.TryGetCompilation(out var compilation))
            {
                var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);
                var xamlNamespaces = XamlNamespaceResolver.ParseNamespaces(syntaxTree);
                var xmlnsDefinitions = XmlnsDefinitionResolver.Resolve(project, platform);
                if (xamlNamespaces != null)
                {
                    viewModel = MvvmResolver.ResolveViewModelSymbol(project, filePath, syntaxTree, compilation, platform, xamlNamespaces, xmlnsDefinitions);
                }
            }

            if (viewModel == null)
            {
                viewModel = MvvmResolver.ResolveViewModelSymbol(project, filePath);
            }

            return viewModel;
        }

        public bool CanNavigateToImplementation(Project project, string filePath, ConfigurationId configId)
        {
            return GetViewModel(project, filePath) != null;
        }

        public IReadOnlyList<IWorkUnit> NavigateToImplementation(Project project, string filePath, ConfigurationId configId)
        {
            var mvvmType = MvvmResolver.ResolveContextType(filePath);
            if (mvvmType == RelationalNavigationContextType.Implementation)
            {
                return new StatusBarMessageWorkUnit()
                {
                    Message = "MFractor: You are already in a ViewModel.",
                }.AsList();
            }

            var viewModel = GetViewModel(project, filePath);

            var syntax = viewModel.GetNonAutogeneratedSyntax();

            var targetProject = GetProjectForSymbol(project.Solution, viewModel);

            return new OpenFileWorkUnit(syntax.SyntaxTree.FilePath, targetProject).AsList();
        }

        public Project GetProjectForSymbol(Solution solution, ISymbol symbol)
        {
            return solution.Projects.FirstOrDefault(p => p.AssemblyName == symbol.ContainingAssembly.Name);
        }

        public bool CanNavigateToDefinition(Project project, string filePath, ConfigurationId configId)
        {
            var view = MvvmResolver.ResolveXamlView(project, filePath);

            return !string.IsNullOrEmpty(view);
        }

        public IReadOnlyList<IWorkUnit> NavigateToDefinition(Project project, string filePath, ConfigurationId configId)
        {
            var mvvmType = MvvmResolver.ResolveContextType(filePath);
            if (mvvmType == RelationalNavigationContextType.Definition)
            {
                return new StatusBarMessageWorkUnit()
                {
                    Message = "MFractor: You are already in a view.",
                }.AsList();
            }

            var view = MvvmResolver.ResolveXamlView(project, filePath);

            return new OpenFileWorkUnit(view, null).AsList();
        }

        public bool CanNavigateToDefinitionCodeBehind(Project project, string filePath, ConfigurationId configId)
        {
            var codeBehind = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);

            if (codeBehind == null)
            {
                return false;
            }

            var syntax = codeBehind.GetNonAutogeneratedSyntax();
            if (syntax == null)
            {
                return false;
            }

            return true;
        }

        public IReadOnlyList<IWorkUnit> NavigateToDefinitionCodeBehind(Project project, string filePath, ConfigurationId configId)
        {
            var codeBehind = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);

            var mvvmType = MvvmResolver.ResolveContextType(filePath);
            if (mvvmType == RelationalNavigationContextType.DefinitionCodeBehind)
            {
                return new StatusBarMessageWorkUnit()
                {
                    Message = "MFractor: You are already in a code-behind class.",
                }.AsList();
            }

            var symbol = MvvmResolver.ResolveCodeBehindSymbol(project, filePath);
            var syntax = symbol.GetNonAutogeneratedSyntax();

            var targetProject = GetProjectForSymbol(project.Solution, symbol);

            return new OpenFileWorkUnit(syntax.SyntaxTree.FilePath, targetProject).AsList();
        }
    }
}