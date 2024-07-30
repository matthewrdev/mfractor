using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Maui.Data.Repositories;
using MFractor.Maui.Utilities;
using MFractor.Maui.XamlPlatforms;
using MFractor.Text;
using MFractor.Utilities;
using MFractor.Utilities.SyntaxWalkers;
using MFractor.Workspace;
using MFractor.Workspace.Data;
using MFractor.Workspace.Data.Models;
using MFractor.Workspace.Data.Repositories;
using MFractor.Workspace.Data.Synchronisation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Data.Synchronisation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class CSharpDocumentSynchroniser : ITextResourceSynchroniser
    {
        static readonly string[] supportedExtensions = { ".cs" };
        public string[] SupportedFileExtensions => supportedExtensions;

        readonly Lazy<IXamlPlatformRepository> xamlPlatforms;
        public IXamlPlatformRepository XamlPlatforms => xamlPlatforms.Value;

        [ImportingConstructor]
        public CSharpDocumentSynchroniser(Lazy<IXamlPlatformRepository> xamlPlatforms)
        {
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
            // Only consider code behind files.
            if (!projectFile.FilePath.EndsWith(".xaml.cs", StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            if (!project.TryGetCompilation(out var compilation))
            {
                return Task.FromResult(false);
            }

            var ast = compilation.SyntaxTrees.FirstOrDefault(st => st.FilePath == projectFile.FilePath);

            return Task.FromResult(true);
        }

        public Task<bool> Synchronise(Solution solution,
                                            Project project,
                                            IProjectFile projectFile,
                                            ITextProvider textProvider,
                                            IProjectResourcesDatabase database)
        {
            if (!project.TryGetCompilation(out var compilation))
            {
                return Task.FromResult(false);
            }

            var syntaxTree = compilation.SyntaxTrees.FirstOrDefault(st => st.FilePath == projectFile.FilePath);

            if (syntaxTree == null)
            {
                return Task.FromResult(false);
            }

            var platform = XamlPlatforms.ResolvePlatform(project);

            if (platform == null)
            {
                // Unsupported or unknown XAML platform.
                return Task.FromResult(false);
            }

            SynchroniseDesignTimeBindingContexts(compilation, syntaxTree, project, projectFile, database);

            SynchroniseDynamicResources(compilation, syntaxTree, platform, projectFile, database);

            return Task.FromResult(true);
        }

        void SynchroniseDynamicResources(Compilation compilation,
                                         SyntaxTree ast,
                                         IXamlPlatform platform,
                                         IProjectFile projectFile,
                                         IProjectResourcesDatabase database)
        {
            var repo = database.GetRepository<DynamicResourceDefinitionRepository>();

            var projectFileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);

            if (projectFileModel == null)
            {
                return;
            }

            var walker = new ClassDeclarationSyntaxWalker();
            walker.Visit(ast.GetRoot());

            var classes = walker.Classes;

            var semanticModel = compilation.GetSemanticModel(ast);

            foreach (var classSyntax in classes)
            {
                var classType = semanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

                var canProcess = SymbolHelper.DerivesFrom(classType, platform.VisualElement.MetaType)
                                 || SymbolHelper.DerivesFrom(classType, platform.Application.MetaType);

                if (!canProcess)
                {
                    continue;
                }

                var methods = classSyntax.Members.OfType<MethodDeclarationSyntax>();
                var constructors = classSyntax.Members.OfType<ConstructorDeclarationSyntax>();

                // TO Resolve:
                // Resources["Key"]
                // Reources[nameof(XXX)]
                // Resources[Class.StringConstant]

                foreach (var m in methods)
                {
                    FindDynamicResources(m.Body, repo, semanticModel, classType, platform, projectFileModel);
                }

                foreach (var c in constructors)
                {
                    FindDynamicResources(c.Body, repo, semanticModel, classType, platform, projectFileModel);
                }
            }
        }

        void FindDynamicResources(BlockSyntax body,
                                  DynamicResourceDefinitionRepository repo,
                                  SemanticModel semanticModel,
                                  INamedTypeSymbol classType,
                                  IXamlPlatform platform,
                                  ProjectFile projectFileModel)
        {
            if (body == null)
            {
                return;
            }

            var expressions = body.Statements.OfType<ExpressionStatementSyntax>();

            if (!expressions.Any())
            {
                return;
            }

            var assignments = expressions.Select(e => e.Expression as AssignmentExpressionSyntax)
                                          .Where(e => e != null);

            foreach (var assingment in assignments)
            {
                try
                {
                    if (assingment.Left is ElementAccessExpressionSyntax eaes)
                    {
                        var identifierName = eaes.Expression as IdentifierNameSyntax;

                        if (identifierName == null)
                        {
                            continue;
                        }

                        var symbol = semanticModel.GetTypeInfo(identifierName);

                        if (identifierName.Identifier.Text != "Resources"
                            || !SymbolHelper.DerivesFrom(symbol.Type, platform.ResourceDictionary.MetaType))
                        {
                            continue;
                        }

                        var name = "";
                        var nameSpan = TextSpan.FromBounds(0, 0);
                        INamedTypeSymbol resourceType = null;

                        var args = eaes.ArgumentList;

                        if (args.Arguments.Count == 1)
                        {
                            var argument = args.Arguments[0];

                            // Resources["key"]
                            if (argument.Expression is LiteralExpressionSyntax les)
                            {
                                name = les.Token.ValueText;
                                nameSpan = TextSpan.FromBounds(les.SpanStart + 1, les.SpanStart + 1 + name.Length);
                            }
                            // Resources[nameof(XXX)]
                            else if (argument.Expression is InvocationExpressionSyntax ies)
                            {
                                var expression = ies.Expression as IdentifierNameSyntax;

                                if (expression == null || expression.Identifier.Text != "nameof")
                                {
                                    continue;
                                }

                                if (ies.ArgumentList.Arguments.Count == 1)
                                {
                                    var a = ies.ArgumentList.Arguments[0];

                                    var ss = semanticModel.GetSymbolInfo(a.Expression).Symbol;

                                    if (ss != null)
                                    {
                                        name = ss.Name;
                                        nameSpan = a.Span;
                                    }
                                }
                            }
                            // Resources[StringConstant]
                            else if (argument.Expression is IdentifierNameSyntax ins)
                            {
                                var stringConstantField = semanticModel.GetSymbolInfo(ins).Symbol as IFieldSymbol;

                                if (stringConstantField != null && stringConstantField.Type.SpecialType == SpecialType.System_String)
                                {
                                    name = stringConstantField.ConstantValue.ToString();
                                    nameSpan = ins.Span;
                                }
                            }
                            else if (argument.Expression is MemberAccessExpressionSyntax maes)
                            {
                                var stringConstantField = semanticModel.GetSymbolInfo(maes).Symbol as IFieldSymbol;

                                if (stringConstantField != null && stringConstantField.Type.SpecialType == SpecialType.System_String)
                                {
                                    name = stringConstantField.ConstantValue.ToString();
                                    nameSpan = maes.Span;
                                }
                            }
                        }

                        resourceType = semanticModel.GetTypeInfo(assingment.Right).Type as INamedTypeSymbol;

                        if (!string.IsNullOrEmpty(name))
                        {
                            var definition = new Models.DynamicResourceDefinition();
                            definition.ProjectFileKey = projectFileModel.PrimaryKey;
                            definition.Name = name;
                            definition.OwnerSymbolMetaType = classType.ToString();
                            definition.NameOffset = nameSpan.Start;
                            definition.NameLength = nameSpan.Length;
                            definition.ExpressionOffset = assingment.Parent.Span.Start;
                            definition.ExpressionLength = assingment.Parent.Span.Length;
                            definition.Expression = assingment.Parent.ToString();
                            definition.ReturnType = resourceType?.ToString();
                            definition.SpecialType = resourceType.SpecialType;
                            repo.Insert(definition);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        void SynchroniseDesignTimeBindingContexts(Compilation compilation,
                                 SyntaxTree ast,
                                 Project project,
                                 IProjectFile projectFile,
                                 IProjectResourcesDatabase database)
        {
            var projectFileModel = database.GetRepository<ProjectFileRepository>().GetProjectFileByFilePath(projectFile.FilePath);
            var repo = database.GetRepository<DesignTimeBindingContextDefinitionRepository>();
            var model = compilation.GetSemanticModel(ast);
            var walker = new ClassDeclarationSyntaxWalker();
            walker.Visit(ast.GetRoot());

            var classes = walker.Classes;

            foreach (var c in classes)
            {
                var symbol = model.GetDeclaredSymbol(c);

                if (symbol is INamedTypeSymbol namedType)
                {
                    var bindingContext = DesignTimeBindingContextHelper.GetTargettedDesignTimeBindingContext(compilation, namedType);

                    if (bindingContext != null)
                    {
                        var entity = new Models.DesignTimeBindingContextDefinition();
                        entity.ProjectFileKey = projectFileModel.PrimaryKey;
                        entity.BindingContextSymbol = bindingContext.ToString();
                        entity.CodeBehindSymbol = symbol.ToString();
                        repo.Insert(entity);
                    }
                }
            }
        }
    }
}
