using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Ide.Navigation;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Syntax.Parsers;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Mvvm
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMvvmResolver))]
    class MvvmResolver : IMvvmResolver
    {
        const string xamlViewExtension = ".xaml";

        readonly Lazy<IMarkupExpressionEvaluater> expressionResolver;
        public IMarkupExpressionEvaluater ExpressionResolver => expressionResolver.Value;

        readonly Lazy<IExpressionParserRepository> expressionParserRepository;
        public IExpressionParserRepository ExpressionParserRepository => expressionParserRepository.Value;

        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        readonly IExpressionParser staticBindingExpressionParser;

        public string DesignTimeBindingContextAttributeName => DesignTimeBindingContextHelper.DesignTimeBindingContextAttributeName;

        public string[] ViewModelSuffixes { get; } = new string[] { "ViewModel", "PageModel", "PageViewModel", "VM", "PageVM" };

        public string[] ViewSuffixes { get; } = new string[] { "", "Page", "View", "Template", "ViewCell" };

        [ImportingConstructor]
        public MvvmResolver(Lazy<IMarkupExpressionEvaluater> expressionResolver,
                            Lazy<IExpressionParserRepository> expressionParserRepository,
                            Lazy<IProjectService> projectService,
                            Lazy<IXamlTypeResolver> xamlTypeResolver)
        {
            this.expressionResolver = expressionResolver;
            this.expressionParserRepository = expressionParserRepository;
            this.projectService = projectService;
            this.xamlTypeResolver = xamlTypeResolver;
            staticBindingExpressionParser = ExpressionParserRepository.ResolveParser(typeof(StaticBindingExpressionParser));
        }

        public RelationalNavigationContextType ResolveContextType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return RelationalNavigationContextType.Unknown;
            }

            var name = Path.GetFileName(filePath);

            var type = RelationalNavigationContextType.Unknown;

            var codeBehindExtension = GetCodeBehindExtension(name);

            // Looks like a xaml view, lets check the file extension and if it has 'Page'.
            if (name.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase))
            {
                type = RelationalNavigationContextType.Definition;
            }
            else if (ViewModelSuffixes.Any(suffix => name.EndsWith(suffix + ".cs", StringComparison.OrdinalIgnoreCase)))
            {
                type = RelationalNavigationContextType.Implementation;
            }
            else if (!string.IsNullOrWhiteSpace(codeBehindExtension))
            {
                type = RelationalNavigationContextType.DefinitionCodeBehind;
            }

            return type;
        }

        string GetCodeBehindExtension(string name)
        {
            if (name.EndsWith(".xaml.cs", StringComparison.OrdinalIgnoreCase))
            {
                return ".xaml.cs";
            }

            if (name.EndsWith(".xaml.g.cs", StringComparison.OrdinalIgnoreCase))
            {
                return ".xaml.g.cs";
            }

            if (name.EndsWith(".xaml.g.i.cs", StringComparison.OrdinalIgnoreCase))
            {
                return ".xaml.g.i.cs";
            }

            return string.Empty;
        }

        public string ResolveXamlView(Project project, string filePath, bool considerProjectReferences = true)
        {
            IProjectFile xamlFile = null;

            var componentName = ExtractComponentName(filePath);

            if (string.IsNullOrEmpty(componentName))
            {
                return null;
            }

            xamlFile = FindView(project, componentName);

            if (xamlFile == null && considerProjectReferences)
            {
                var solution = project.Solution;

                var projects = solution.Projects.Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id));

                if (projects.Any())
                {
                    foreach (var p in projects)
                    {
                        xamlFile = FindView(p, componentName);

                        if (xamlFile != null)
                        {
                            break;
                        }
                    }
                }
            }

            return xamlFile?.FilePath;
        }

        IProjectFile FindView(Project project, string componentName)
        {
            IProjectFile xamlFile = null;

            foreach (var s in ViewSuffixes)
            {
                var searchName = componentName + s + ".xaml";

                xamlFile = ProjectService.FindProjectFile(project, (pf => pf.EndsWith(searchName, StringComparison.OrdinalIgnoreCase)));

                if (xamlFile != null)
                {
                    break;
                }
            }

            return xamlFile;
        }

        public string ExtractComponentName(string filePath, RelationalNavigationContextType contextType)
        {
            var name = Path.GetFileName(filePath);
            var extension = string.Empty;

            switch (contextType)
            {
                case RelationalNavigationContextType.DefinitionCodeBehind:
                    extension = GetCodeBehindExtension(filePath);
                    break;
                case RelationalNavigationContextType.Implementation:
                    foreach (var suffix in ViewModelSuffixes)
                    {
                        if (name.EndsWith(suffix + ".cs", System.StringComparison.OrdinalIgnoreCase))
                        {
                            extension = suffix + ".cs";
                            break;
                        }
                    }
                    break;
                case RelationalNavigationContextType.Definition:
                    extension = xamlViewExtension;
                    break;
            }

            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var componentName = name.Substring(0, name.Length - extension.Length);
            return componentName;
        }

        public string ExtractComponentName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            var contextType = ResolveContextType(filePath);

            return ExtractComponentName(filePath, contextType);
        }

        public INamedTypeSymbol ResolveViewModelSymbol(Project project, string filePath, bool considerProjectReferences = true)
        {
            var bindingContext = ResolveDesignTimeBindingContext(project, filePath);
            if (bindingContext != null)
            {
                return bindingContext;
            }

            var componentName = ExtractComponentName(filePath);

            if (string.IsNullOrEmpty(componentName))
            {
                return null;
            }

            bindingContext = FindMatchingViewModel(componentName, project);

            if (bindingContext is null && considerProjectReferences)
            {
                var solution = project.Solution;

                var projects = solution.Projects.Where(p => project.ProjectReferences.Any(pr => pr.ProjectId == p.Id));

                if (projects.Any())
                {
                    foreach (var p in projects)
                    {
                        bindingContext = FindMatchingViewModel(componentName, p);

                        if (bindingContext != null)
                        {
                            return bindingContext;
                        }
                    }
                }
            }

            return bindingContext;
        }

        INamedTypeSymbol FindMatchingViewModel(string componentName, Project project)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return default;
            }

            INamedTypeSymbol bindingContext = null;
            foreach (var suffix in ViewModelSuffixes)
            {
                var viewModelName = componentName + suffix;

                bindingContext = TryResolveClass(compilation, viewModelName);
                if (bindingContext == null) // Attes
                {
                    var retry = false;
                    if (componentName.EndsWith("page", System.StringComparison.OrdinalIgnoreCase))
                    {
                        viewModelName = componentName.Remove(componentName.Length - "page".Length, "page".Length) + suffix;
                        retry = true;
                    }
                    else if (componentName.EndsWith("view", System.StringComparison.OrdinalIgnoreCase))
                    {
                        viewModelName = componentName.Remove(componentName.Length - "view".Length, "view".Length) + suffix;
                        retry = true;
                    }
                    else if (componentName.EndsWith("template", System.StringComparison.OrdinalIgnoreCase))
                    {
                        viewModelName = componentName.Remove(componentName.Length - "template".Length, "template".Length) + suffix;
                        retry = true;
                    }

                    if (retry)
                    {
                        bindingContext = TryResolveClass(compilation, viewModelName);
                    }
                }

                if (bindingContext != null)
                {
                    break;
                }
            }

            return bindingContext;
        }

        public INamedTypeSymbol ResolveCodeBehindSymbol(Project project, string filePath, bool considerProjectReferences = true)
        {
            var mvvmContextType = ResolveContextType(filePath);

            var componentName = ExtractComponentName(filePath, mvvmContextType);

            if (string.IsNullOrEmpty(componentName))
            {
                return null;
            }

            var codeBehind = TryResolveClassUsingSuffixes(project, componentName, ViewSuffixes);

            if (codeBehind is null)
            {
                var solution = project.Solution;

                var projects = solution.Projects.Where(p => p.ProjectReferences.Any(pr => pr.ProjectId == project.Id));

                if (projects.Any())
                {
                    foreach (var p in projects)
                    {
                        codeBehind = TryResolveClassUsingSuffixes(p, componentName, ViewSuffixes);

                        if (codeBehind != null)
                        {
                            break;
                        }
                    }
                }
            }

            return codeBehind;
        }

        public bool IsXamarinFormsXamlViewContext(Project project, string filePath)
        {
            var file = ProjectService.GetProjectFileWithFilePath(project, filePath);

            if (file == null)
            {
                return false;
            }

            return true;
        }

        public INamedTypeSymbol TryResolveClassUsingSuffixes(Project project, string className, IEnumerable<string> suffixes)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var symbol = TryResolveClass(compilation, className);

            if (symbol == null)
            {
                foreach (var s in suffixes)
                {
                    var transformedClassName = className + s;
                    symbol = TryResolveClass(compilation, transformedClassName);
                    if (symbol != null)
                    {
                        break;
                    }
                }
            }

            return symbol;
        }

        public INamedTypeSymbol TryResolveClass(Compilation compilation, string className)
        {
            if (compilation == null)
            {
                return null;
            }

            var type = compilation.GetSymbolsWithName((name) =>
            {
                return name == className;
            }, SymbolFilter.Type).OfType<INamedTypeSymbol>()
                                 .FirstOrDefault(symbol => symbol.ContainingAssembly == compilation.Assembly);

            return type;
        }

        public INamedTypeSymbol ResolveViewModelSymbol(Project project,
                                                       string filePath,
                                                       XmlSyntaxTree syntaxTree,
                                                       Compilation compilation,
                                                       IXamlPlatform platform,
                                                       IXamlNamespaceCollection namespaces,
                                                       IXmlnsDefinitionCollection xmlnsDefinitions,
                                                       bool considerProjectReferences = true)
        {
            INamedTypeSymbol documentBindingContext = null;

            var expectedBindingContextName = $"{syntaxTree.Root.Name.FullName}.{platform.BindingContextProperty}";

            var bindingContextAttr = syntaxTree.Root.GetAttribute(attr =>
            {
                if (attr.Name == null)
                {
                    return false;
                }

                return attr.Name.FullName == platform.BindingContextProperty;
            });
            var bindingContextChild = syntaxTree.Root.GetChildren(node =>
            {
                if (node.Name == null)
                {
                    return false;
                }

                return node.Name.FullName == expectedBindingContextName;
            }).FirstOrDefault();
            var dataTypeAttr = syntaxTree.Root.GetAttributeByName("x:DataType");

            if (bindingContextAttr != null)
            {
                var span = bindingContextAttr.Span;
                var staticExpression = staticBindingExpressionParser.Parse(bindingContextAttr.Value?.Value, span.Start, span.End, null, null, project, namespaces, xmlnsDefinitions, platform);

                var resolveResult = ExpressionResolver.EvaluateStaticBindingExpression(project, platform, namespaces, xmlnsDefinitions, staticExpression as StaticBindingExpression);

                if (resolveResult != null
                    && resolveResult.Symbol != null)
                {
                    var bindingContextType = SymbolHelper.ResolveMemberReturnType(resolveResult.Symbol as ISymbol);
                    if (bindingContextType != null)
                    {
                        documentBindingContext = bindingContextType as INamedTypeSymbol;
                    }
                }
            }
            else if (bindingContextChild != null
                     && bindingContextChild.HasChildren)
            {
                var inner = bindingContextChild.Children.First();

                var childNamespace = namespaces.ResolveNamespace(inner.Name.Namespace);

                if (childNamespace != null)
                {
                    documentBindingContext = XamlTypeResolver.ResolveType(inner.Name.LocalName, childNamespace, project, xmlnsDefinitions);
                }
            }
            else if (dataTypeAttr != null)
            {
                documentBindingContext = ResolveDataType(project, namespaces, xmlnsDefinitions, dataTypeAttr);
            }
            else
            {
                documentBindingContext = ResolveViewModelSymbol(project, filePath);
            }

            return documentBindingContext;
        }

        INamedTypeSymbol ResolveDataType(Project project, IXamlNamespaceCollection namespaces, IXmlnsDefinitionCollection xmlnsDefinitions, XmlAttribute dataTypeAttribute)
        {
            var symbol = dataTypeAttribute?.Value?.Value;

            if (string.IsNullOrEmpty(symbol))
            {
                return default;
            }

            if (!XamlSyntaxHelper.ExplodeTypeReference(symbol, out var xmlns, out var className))
            {
                return default;
            }

            var xmlNamespace = namespaces.ResolveNamespace(xmlns);

            return XamlTypeResolver.ResolveType(className, xmlNamespace, project, xmlnsDefinitions);
        }

        public INamedTypeSymbol ResolveDesignTimeBindingContext(Project project, string filePath)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return null;
            }

            var codeBehind = ResolveCodeBehindSymbol(project, filePath);

            if (codeBehind == null)
            {
                return null;
            }

            return ResolveDesignTimeBindingContext(compilation, codeBehind);
        }

        public INamedTypeSymbol ResolveDesignTimeBindingContext(Compilation compilation, INamedTypeSymbol codeBehind)
        {
            return DesignTimeBindingContextHelper.GetTargettedDesignTimeBindingContext(compilation, codeBehind);
        }
    }
}
