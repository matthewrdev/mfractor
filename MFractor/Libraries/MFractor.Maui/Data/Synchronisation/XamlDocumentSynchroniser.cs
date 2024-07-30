using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.Formatting;
using MFractor.Maui.Data.Models;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Semantics;
using MFractor.Maui.Symbols;
using MFractor.Maui.Syntax;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Models;
using MFractor.Workspace.Data.Repositories;
using MFractor.Workspace.Data.Synchronisation;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class XamlDocumentSynchroniser : ITextResourceSynchroniser
    {
        public string[] SupportedFileExtensions { get; } = { ".xaml" };

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        public ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService;
        public IXmlFormattingPolicyService XmlFormattingPolicyService => xmlFormattingPolicyService.Value;

        readonly Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory;
        public IXamlSemanticModelFactory XamlSemanticModelFactory => xamlSemanticModelFactory.Value;

        readonly Lazy<IXamlTypeResolver> xamlTypeResolver;
        public IXamlTypeResolver XamlTypeResolver => xamlTypeResolver.Value;

        readonly Lazy<IParsedXamlDocumentFactory> parsedXamlDocumentFactory;
        public IParsedXamlDocumentFactory ParsedXamlDocumentFactory => parsedXamlDocumentFactory.Value;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public XamlDocumentSynchroniser(Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                        Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                        Lazy<IXamlTypeResolver> xamlTypeResolver,
                                        Lazy<IParsedXamlDocumentFactory> parsedXamlDocumentFactory,
                                        Lazy<IXamlSemanticModelFactory> xamlSemanticModelFactory,
                                        Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService,
                                        Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.xamlTypeResolver = xamlTypeResolver;
            this.parsedXamlDocumentFactory = parsedXamlDocumentFactory;
            this.xamlSemanticModelFactory = xamlSemanticModelFactory;
            this.xmlFormattingPolicyService = xmlFormattingPolicyService;
            this.xamlPlatforms = xamlPlatforms;
        }

        public bool IsAvailable(Solution solution, Project project)
        {
            return XamlPlatforms.CanResolvePlatform(project);
        }

        public Task<bool> CanSynchronise(Solution solution,
                                   Project project,
                                   IProjectFile projectFile)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> Synchronise(Solution solution,
                                            Project project,
                                            IProjectFile projectFile,
                                            ITextProvider textProvider,
                                            IProjectResourcesDatabase database)
        {
            var compilation = await project.GetCompilationAsync();
            var document = await ParsedXamlDocumentFactory.CreateAsync(project, projectFile, textProvider);

            var projectFileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);

            if (document is null || projectFileModel is null || compilation is null)
            {
                return false;
            }

            var syntaxTree = document.XamlSyntaxTree;

            var platform = XamlPlatforms.ResolvePlatform(project, compilation, syntaxTree);

            var semanticModel = XamlSemanticModelFactory.Create(document, project);

            var classDeclaration = syntaxTree.Root.GetAttributeByName("x:Class");

            if (classDeclaration != null && classDeclaration.HasValue)
            {
                var repo = database.GetRepository<ClassDeclarationRepository>();
                var declaration = new ClassDeclaration();
                declaration.MetaDataName = classDeclaration.Value.Value;
                declaration.StartOffset = classDeclaration.Value.Span.Start;
                declaration.EndOffset = classDeclaration.Value.Span.End;
                declaration.ProjectFileKey = projectFileModel.PrimaryKey;
                repo.Insert(declaration);
            }

            SynchroniseStaticResources(projectFileModel, database, project, document, semanticModel, platform);

            SynchroniseResourceDictionaryReferences(project, projectFileModel, database, semanticModel, document, platform);

            SynchroniseAutomationIds(syntaxTree.Root, projectFileModel, database, semanticModel, document, platform);

            SynchroniseElementUsages(syntaxTree.Root, projectFileModel, semanticModel, platform, database.GetRepository<ThicknessUsageRepository>(), database.GetRepository<ColorUsageRepository>());

            return true;
        }

        void SynchroniseAutomationIds(XmlNode node,
                                      ProjectFile projectFile,
                                      IProjectResourcesDatabase database,
                                      IXamlSemanticModel semanticModel,
                                      IParsedXamlDocument document,
                                      IXamlPlatform platform)
        {
            if (node == null)
            {
                return;
            }

            if (node.HasChildren)
            {
                foreach (var c in node.Children)
                {
                    SynchroniseAutomationIds(c, projectFile, database, semanticModel, document, platform);
                }
            }

            if (!XamlSyntaxHelper.IsPropertySetter(node) && node.HasAttributes)
            {
                var automationIdAttr = node.GetAttribute(document.Namespaces.DefaultNamespace.Prefix, "AutomationId");

                if (automationIdAttr != null && automationIdAttr.HasValue)
                {
                    var automationId = new AutomationIdDeclaration
                    {
                        ProjectFileKey = projectFile.PrimaryKey,
                        Name = automationIdAttr.Value.Value,
                        StartOffset = automationIdAttr.Value.Span.Start,
                        EndOffset = automationIdAttr.Value.Span.End,
                        ParentSymbolName = node.Name.LocalName
                    };

                    var symbol = semanticModel.GetSymbol(node) as INamedTypeSymbol;
                    if (symbol!= null)
                    {
                        automationId.ParentSymbolNamespace = symbol.ContainingNamespace.ToString();
                    }

                    database.Insert(automationId);
                }
            }
        }

        void SynchroniseResourceDictionaryReferences(Project project,
                                                     ProjectFile projectFile,
                                                     IProjectResourcesDatabase database,
                                                     IXamlSemanticModel semanticModel,
                                                     IParsedXamlDocument document,
                                                     IXamlPlatform platform)
        {
            var root = document.XamlSyntaxRoot;

            var resourceNodes = new List<XmlNode>();
            var resourcesSetter = root.GetChildNode(c => c.Name.LocalName.EndsWith(".Resources", StringComparison.InvariantCulture));

            if (resourcesSetter == null)
            {
                if (root.Name.LocalName == "ResourceDictionary")
                {
                    resourceNodes.Add(root);
                }
            }
            else
            {
                resourceNodes.Add(resourcesSetter);
                var nodes = resourcesSetter.GetChildren(c => c.Name.LocalName == "ResourceDictionary");

                if (nodes != null && nodes.Any())
                {
                    resourceNodes.AddRange(nodes.Where(n => n != null));
                }
            }

            foreach (var node in resourceNodes)
            {
                SynchroniseXmlNodeResourceDictionaryReferences(node, projectFile, project, database, semanticModel, document);
            }
        }

        void SynchroniseXmlNodeResourceDictionaryReferences(XmlNode node,
                                                            ProjectFile projectFile,
                                                            Project project,
                                                            IProjectResourcesDatabase database,
                                                            IXamlSemanticModel semanticModel,
                                                            IParsedXamlDocument document)
        {
            const string MergedDictionaries = "ResourceDictionary.MergedDictionaries";
            const string MergedWith = "MergedWith";
            const string Source = "Source";

            var mergedDictionaries = node.GetChildNode(c => c.Name.LocalName == MergedDictionaries);
            var mergedWithAttr = node.GetAttribute(c => c.Name.LocalName == MergedWith);
            var sourceAttr = node.GetAttribute(c => c.Name.LocalName == Source);

            if (mergedDictionaries != null && mergedDictionaries.HasChildren)
            {
                var children = mergedDictionaries.Children;

                foreach (var child in children)
                {
                    var source = child.GetAttribute(c => c.Name.LocalName == Source);

                    if (source != null)
                    {
                        CreateFilePathResourceDictionaryReference(projectFile, project, database, source);
                    }
                    else
                    {
                        var reference = new ResourceDictionaryReference
                        {
                            SymbolName = child.Name.LocalName,
                            ProjectFileKey = projectFile.PrimaryKey
                        };

                        var symbol = semanticModel.GetSymbol(child) as INamedTypeSymbol;

                        if (symbol != null)
                        {
                            reference.SymbolNamespace = symbol.ContainingNamespace.ToString();
                        }

                        database.Insert(reference);
                    }
                }
            }

            if (mergedWithAttr != null && mergedWithAttr.HasValue)
            {
                if (XamlSyntaxHelper.ExplodeTypeReference(mergedWithAttr.Value.Value, out var namespaceName, out var className))
                {
                    var reference = new ResourceDictionaryReference
                    {
                        SymbolName = className,
                        ProjectFileKey = projectFile.PrimaryKey
                    };

                    var xmlns = document.Namespaces.ResolveNamespace(namespaceName);

                    var symbol = XamlTypeResolver.ResolveType(namespaceName, xmlns, project, document.XmlnsDefinitions);
                    if (symbol != null)
                    {
                        reference.SymbolNamespace = symbol.ContainingNamespace.ToString();
                    }

                    database.Insert(reference);
                }
            }

            if (sourceAttr != null && sourceAttr.HasValue)
            {
                CreateFilePathResourceDictionaryReference(projectFile, project, database, sourceAttr);
            }
        }

        void CreateFilePathResourceDictionaryReference(ProjectFile projectFile, Project project, IProjectResourcesDatabase database, XmlAttribute sourceAttr)
        {
            var projectInfo = new FileInfo(project.FilePath);
            var reference = new ResourceDictionaryReference();

            var fileName = sourceAttr.Value.Value;
            var kind = DictionaryReferenceKind.FileName;

            if (fileName.StartsWith("/", StringComparison.Ordinal)
                || fileName.StartsWith("\\", StringComparison.Ordinal))
            {
                kind = DictionaryReferenceKind.FilePath;
                var projectDirectory = projectInfo.Directory.FullName;

                fileName = fileName.Remove(0, 1);

                fileName = Path.Combine(projectDirectory, fileName);
            }
            else if (fileName.StartsWith("..\\", StringComparison.Ordinal)
                    || fileName.StartsWith("../", StringComparison.Ordinal))
            {
                kind = DictionaryReferenceKind.FilePath;

                var fileInfo = new FileInfo(projectFile.FilePath);

                fileName = Path.Combine(fileInfo.Directory.FullName, fileName);
                fileName = Path.GetFullPath((new Uri(fileName)).LocalPath);
            }

            reference.FileName = fileName;
            reference.ProjectFileKey = projectFile.PrimaryKey;
            reference.ReferenceKind = kind;

            database.Insert(reference);
        }

        void SynchroniseStaticResources(ProjectFile projectFile,
                                        IProjectResourcesDatabase database,
                                        Project project,
                                        IParsedXamlDocument document,
                                        IXamlSemanticModel semanticModel,
                                        IXamlPlatform platform)
        {
            var root = document.XamlSyntaxRoot;

            var resourceNodes = new List<XmlNode>();
            var resourcesSetter = root.GetChildNode(c => c.Name.LocalName.EndsWith(".Resources", StringComparison.InvariantCulture));

            if (resourcesSetter == null)
            {
                if (root.Name.LocalName == "ResourceDictionary")
                {
                    resourceNodes.Add(root);
                }
            }
            else
            {
                resourceNodes.Add(resourcesSetter);
                var node = resourcesSetter.GetChildNode(c => c.Name.LocalName == "ResourceDictionary");

                if (node != null)
                {
                    resourceNodes.Add(node);
                }
            }

            foreach (var node in resourceNodes)
            {
                if (node.HasChildren)
                {
                    SynchroniseResourcesUsingXmlNode(node, projectFile, database, project, document, semanticModel, platform);
                }
            }
        }

        void SynchroniseResourcesUsingXmlNode(XmlNode node,
                                              ProjectFile projectFile,
                                              IProjectResourcesDatabase database,
                                              Project project,
                                              IParsedXamlDocument document,
                                              IXamlSemanticModel semanticModel,
                                              IXamlPlatform platform)
        {
            var styleDefinitionRepo = database.GetRepository<StyleDefinitionRepository>();
            var staticResourceDefinitionRepo = database.GetRepository<StaticResourceDefinitionRepository>();
            var styleSetterRepo = database.GetRepository<StyleSetterRepository>();
            var colorDefinitionRepo = database.GetRepository<ColorDefinitionRepository>();
            var thicknessDefinitionRepo = database.GetRepository<ThicknessDefinitionRepository>();
            var onPlatformDeclarationRepository = database.GetRepository<OnPlatformDeclarationRepository>();
            var stringDefinitionRepo = database.GetRepository<StringResourceDefinitionRepository>();

            foreach (var childSyntax in node.Children)
            {
                if (XamlSyntaxHelper.IsPropertySetter(childSyntax))
                {
                    continue;
                }

                var resource = new StaticResourceDefinition
                {
                    ProjectFileKey = projectFile.PrimaryKey,
                    SymbolName = childSyntax.Name.LocalName
                };

                var resourceSymbol = semanticModel.GetSymbol(childSyntax) as INamedTypeSymbol;

                if (resourceSymbol != null)
                {
                    resource.SymbolNamespace = resourceSymbol.ContainingNamespace.ToString();
                    resource.SymbolAssembly = resourceSymbol.ContainingAssembly.Name;
                }

                resource.ReturnType = GetReturnType(childSyntax, document, project, resource, platform);
                resource.TargetType = GetTypeDefinedByAttribute(childSyntax, project, document, "TargetType");

                var key = childSyntax.GetAttributeByName("x:Key");
                if (key != null)
                {
                    resource.Name = key.Value?.Value;

                    if (key.Value != null)
                    {
                        resource.NameStart = key.Value.Span.Start;
                        resource.NameEnd = key.Value.Span.End;
                    }
                }

                resource.StartOffset = childSyntax.Span.Start;
                resource.EndOffset = childSyntax.Span.End;
                resource.PreviewString = XmlSyntaxWriter.WriteNode(childSyntax, string.Empty, XmlFormattingPolicyService.GetXmlFormattingPolicy(), true, true, true);
                resource.IsStyleMetaType = SymbolHelper.DerivesFrom(resourceSymbol, platform.Style.MetaType);

                database.Insert(resource);

                if (resource.SymbolMetaType == platform.Style.MetaType)
                {
                    CreateStyleDefinition(childSyntax, styleDefinitionRepo, styleSetterRepo, resource, platform);
                }

                if (resource.SymbolMetaType == platform.Color.MetaType
                    || resource.SymbolMetaType == "System.Drawing.Color")
                {
                    CreateColorDefinition(childSyntax, colorDefinitionRepo, resource);
                }

                if (resource.SymbolMetaType == platform.Thickness.MetaType)
                {
                    CreateThicknessDefinition(childSyntax, thicknessDefinitionRepo, resource);
                }

                if (resource.SymbolMetaType == "System.String")
                {
                    CreateStringResourceDefinition(childSyntax, stringDefinitionRepo, resource);
                }

                if (resource.SymbolName == platform.OnPlatform.NonGenericName)
                {
                    CreateOnPlatformDeclaration(childSyntax, onPlatformDeclarationRepository, resource, project, document);
                }
            }
        }

        void CreateStringResourceDefinition(XmlNode syntax, StringResourceDefinitionRepository stringDefinitionRepo, StaticResourceDefinition resource)
        {
            if (!syntax.HasValue)
            {
                return;
            }

            var definition = new StringResourceDefinition();
            definition.Value = syntax.Value;
            definition.Name = resource.Name;
            definition.ValueStart = syntax.ValueSpan.Start;
            definition.ValueEnd = syntax.ValueSpan.End;
            definition.ProjectFileKey = resource.ProjectFileKey;
            definition.StaticResourceKey = resource.PrimaryKey;
            stringDefinitionRepo.Insert(definition);
        }

        void CreateOnPlatformDeclaration(XmlNode syntax,
                                         OnPlatformDeclarationRepository repo,
                                         StaticResourceDefinition resource,
                                         Project project,
                                         IParsedXamlDocument document)
        {
            var platforms = new Dictionary<string, string>();
            foreach (var child in syntax.GetChildren(c => c.Name.FullName == "On"))
            {
                var platform = child.GetAttributeByName("Platform");
                var value = child.GetAttributeByName("Value");
                if (platform != null && platform.HasValue)
                {
                    platforms[platform.Value.Value] = value?.Value?.Value;
                }
            }

            var declaration = new OnPlatformDeclaration();
            declaration.Platforms = platforms;
            declaration.Name = resource.Name;
            declaration.Type = GetTypeDefinedByAttribute(syntax, project, document, "x:TypeArguments");
            declaration.ProjectFileKey = resource.ProjectFileKey;
            declaration.StaticResourceKey = resource.PrimaryKey;
            repo.Insert(declaration);
        }

        void CreateColorDefinition(XmlNode syntax, ColorDefinitionRepository colorDefinitionRepo, StaticResourceDefinition resource)
        {
            if (!syntax.HasValue)
            {
                return;
            }

            Color color;
            try
            {
                color = ColorTranslator.FromHtml(syntax.Value);
            }
            catch
            {
                return;
            }

            var colorDefinition = new ColorDefinition();
            colorDefinition.Color = color;
            colorDefinition.Name = resource.Name;
            colorDefinition.ProjectFileKey = resource.ProjectFileKey;
            colorDefinition.StaticResourceKey = resource.PrimaryKey;
            colorDefinitionRepo.Insert(colorDefinition);
        }

        void CreateThicknessDefinition(XmlNode syntax, ThicknessDefinitionRepository thicknessDefinitionRepository, StaticResourceDefinition resource)
        {
            if (!ThicknessHelper.ProcessThickness(syntax, out var left, out var right, out var top, out var bottom))
            {
                return;
            }

            var definition = new ThicknessDefinition();
            definition.Left = left;
            definition.Right = right;
            definition.Top = top;
            definition.Bottom = bottom;
            definition.Name = resource.Name;
            definition.ProjectFileKey = resource.ProjectFileKey;
            definition.StaticResourceKey = resource.PrimaryKey;
            thicknessDefinitionRepository.Insert(definition);
        }

        void CreateStyleDefinition(XmlNode syntax, StyleDefinitionRepository styleDefinitionRepo, StyleSetterRepository styleSetterRepo, StaticResourceDefinition resource, IXamlPlatform platform)
        {
            var style = new StyleDefinition();
            style.StartOffset = resource.StartOffset;
            style.EndOffset = resource.EndOffset;
            style.Name = resource.Name;
            style.NameStart = resource.NameStart;
            style.NameEnd = resource.NameEnd;
            style.TargetType = resource.TargetType;
            style.ProjectFileKey = resource.ProjectFileKey;
            style.StaticResourceId = resource.PrimaryKey;
            style.BaseStyleName = GetBaseStyleName(syntax, platform);
            styleDefinitionRepo.Insert(style);

            var setters = syntax.GetChildren(c => c.Name.FullName == "Setter");

            if (setters.Any())
            {
                foreach (var s in setters)
                {
                    var setter = new StyleSetter();
                    setter.ProjectFileKey = style.ProjectFileKey;
                    setter.StyleDefinitionKey = style.PrimaryKey;

                    if (s.HasAttribute("Property"))
                    {
                        var property = s.GetAttributeByName("Property");

                        if (property.HasValue)
                        {
                            setter.Property = property.Value.Value;
                            setter.PropertyOffset = property.Value.Span.Start;
                            setter.PropertyLength = property.Value.Span.Length;
                        }
                    }

                    if (s.HasAttribute("Value"))
                    {
                        var value = s.GetAttributeByName("Value");

                        if (value.HasValue)
                        {
                            setter.Value = value.Value.Value;
                            setter.ValueOffset = value.Value.Span.Start;
                            setter.ValueLength = value.Value.Span.Length;
                        }
                    }

                    styleSetterRepo.Insert(setter);
                }
            }
        }

        string GetBaseStyleName(XmlNode syntax, IXamlPlatform platform)
        {
            var baseKey = syntax.GetAttributeByName("BaseResourceKey");

            if (baseKey != null)
            {
                return baseKey.Value?.Value;
            }

            var basedOn = syntax.GetAttributeByName("BasedOn");

            if (basedOn != null && ExpressionParserHelper.IsExpression(basedOn.Value?.Value))
            {
                var expressionParser = new XamlExpressionParser(basedOn.Value.Value, basedOn.Value.Span.Start);

                var expression = expressionParser.Parse();

                if (expression is ExpressionSyntax expressionSyntax
                    && expressionSyntax.NameSyntax is TypeNameSyntax typeNameSyntax
                    && typeNameSyntax.Name == platform.StaticResourceExtension.MarkupExpressionName
                    && expressionSyntax.Elements.FirstOrDefault() is ContentSyntax contentSyntax)
                {
                    return contentSyntax.Content;
                }
            }

            return string.Empty;
        }

        string GetReturnType(XmlNode child,
                             IParsedXamlDocument document,
                             Project project,
                             StaticResourceDefinition resource,
                             IXamlPlatform platform)
        {
            if (child == null || project == null)
            {
                return string.Empty;
            }

            var microsoft = document.Namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);
            var xmlns = document.Namespaces.ResolveNamespace(child.Name.Namespace);

            if (XamlSchemaHelper.IsPlatformSchema(xmlns, platform)
                && (child.Name.LocalName == platform.OnPlatform.Name || child.Name.LocalName == platform.OnIdiom.Name))
            {
                var typeArgsAttr = child.GetAttribute(a => a.Name.Namespace == microsoft.Prefix && a.Name.LocalName == Keywords.MicrosoftSchema.TypeArguments);

                if (typeArgsAttr != null
                    && XamlSyntaxHelper.ExplodeTypeReference(typeArgsAttr.Value?.Value, out var namespaceName, out var className))
                {
                    return GetFullyQualifiedSymbolName(namespaceName, className, project, document);
                }
            }
            else
            {
                return resource.SymbolMetaType;
            }

            return string.Empty;
        }

        string GetTypeDefinedByAttribute(XmlNode child, Project project, IParsedXamlDocument document, string atributeName)
        {
            if (child == null || document == null)
            {
                return string.Empty;
            }

            var targetAttr = child.GetAttributeByName(atributeName);
            if (targetAttr == null || !targetAttr.HasValue)
            {
                return string.Empty;
            }

            var value = targetAttr.Value.Value;

            if (XamlSyntaxHelper.ExplodeTypeReference(value, out var namespaceName, out var className))
            {
                return GetFullyQualifiedSymbolName(namespaceName, className, project, document);
            }

            return string.Empty;
        }

        string GetFullyQualifiedSymbolName(string xmlns, string className, Project project, IParsedXamlDocument document)
        {
            var ns = document.Namespaces.ResolveNamespace(xmlns);
            
            if (ns == null)
            {
                return string.Empty;
            }

            return XamlTypeResolver.ResolveType(className, ns, project, document.XmlnsDefinitions)?.ToString();
        }

        void SynchroniseElementUsages(XmlNode syntax, ProjectFile projectFile, IXamlSemanticModel semanticModel, IXamlPlatform platform, ThicknessUsageRepository thicknessUsageRepository, ColorUsageRepository colorUsageRepository)
        {
            var resourceNodes = new List<XmlNode>();
            var resourcesSetter = syntax.GetChildNode(c => c.Name.LocalName.EndsWith(".Resources", StringComparison.InvariantCulture));

            if (syntax.HasValue)
            {
                if (!ExpressionParserHelper.IsExpression(syntax.Value))
                {
                    if (syntax.Name.FullName == "Thickness")
                    {
                        if (ThicknessHelper.ProcessThickness(syntax.Value, out var thickness))
                        {
                            var usage = new ThicknessUsage();
                            usage.Thickness = thickness;
                            usage.ValueSpan = syntax.ValueSpan;
                            usage.ProjectFileKey = projectFile.PrimaryKey;
                            usage.Value = syntax.Value;
                            thicknessUsageRepository.Insert(usage);
                        }
                    }
                    else if (syntax.Name.FullName == "Color")
                    {
                        if (ColorHelper.TryEvaluateColor(syntax.Value, out var color))
                        {
                            var usage = new ColorUsage();
                            usage.Color = color;
                            usage.IsHexColor = ColorHelper.TryParseHexColor(syntax.Value, out _, out _);
                            usage.ValueSpan = syntax.ValueSpan;
                            usage.ProjectFileKey = projectFile.PrimaryKey;
                            usage.Value = syntax.Value;
                            colorUsageRepository.Insert(usage);
                        }
                    }
                }
            }

            if (syntax.HasChildren)
            {
                foreach (var node in syntax.Children)
                {
                    SynchroniseElementUsages(node, projectFile, semanticModel, platform, thicknessUsageRepository, colorUsageRepository);
                }
            }

            var parentType = semanticModel.GetSymbol(syntax) as INamedTypeSymbol;

            if (syntax.HasAttributes && parentType != null)
            {
                foreach (var attr in syntax.Attributes)
                {
                    if (attr.HasValue)
                    {
                        if (!ExpressionParserHelper.IsExpression(attr.Value.Value))
                        {
                            var member = SymbolHelper.FindMemberSymbolByName(parentType, attr.Name.FullName);
                            var type = SymbolHelper.ResolveMemberReturnType(member);

                            if (SymbolHelper.DerivesFrom(type, platform.Thickness.MetaType))
                            {
                                CreateThicknessUsage(attr.Value.Value, attr.Value.Span, projectFile, thicknessUsageRepository);
                            }
                            else if (SymbolHelper.DerivesFrom(type, platform.Color.MetaType))
                            {
                                CreateColorUsage(attr.Value.Value, attr.Value.Span, projectFile, colorUsageRepository);
                            }
                        }
                    }
                }
            }
        }

        void CreateColorUsage(string value, TextSpan span, ProjectFile projectFile, ColorUsageRepository colorUsageRepository)
        {
            if (ColorHelper.TryEvaluateColor(value, out var color))
            {
                var usage = new ColorUsage();
                usage.Color = color;
                usage.IsHexColor = ColorHelper.TryParseHexColor(value, out _, out _);
                usage.ValueSpan = span;
                usage.ProjectFileKey = projectFile.PrimaryKey;
                usage.Value = value;
                colorUsageRepository.Insert(usage);
            }
        }

        void CreateThicknessUsage(string value, TextSpan span, ProjectFile projectFile, ThicknessUsageRepository thicknessUsageRepository)
        {
            if (ThicknessHelper.ProcessThickness(value, out var thickness))
            {
                var usage = new ThicknessUsage();
                usage.Thickness = thickness;
                usage.ValueSpan = span;
                usage.ProjectFileKey = projectFile.PrimaryKey;
                usage.Value = value;
                thicknessUsageRepository.Insert(usage);
            }
        }
    }
}
