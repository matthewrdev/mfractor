using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration.ClassFromClipboard
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IClassFromStringContentGenerator))]
    class ClassFromStringContentGenerator : CodeGenerator, IClassFromStringContentGenerator
    {
        [Import]
        public IClassDeclarationGenerator ClassDeclarationGenerator { get; set; }

        [Import]
        public INamespaceDeclarationGenerator NamespaceDeclarationGenerator { get; set; }

        [Import]
        public IUsingDirectiveGenerator UsingDirectiveGenerator { get; set; }

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Identifier => "com.mfractor.code_gen.csharp.class_from_string_content";

        public override string Name => "Create Class From String Content Code Generator";

        public override string Documentation => "Given a string buffer or syntax tree, this code generator can generate a new project file based on that content. If a project, folder path and alternative class name is provided, this code generator can change the namespace and class name of the content given alternate values.";

        public bool CanCreateClassFromContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            return CanCreateClassFromContent(syntaxTree);
        }

        public bool CanCreateClassFromContent(CompilationUnitSyntax ast)
        {
            if (ast == null)
            {
                return false;
            }

            var type = GetAvailableTypeSyntax(ast);

            return type != null && type.Identifier != null && !string.IsNullOrEmpty(type.Identifier.ValueText);
        }

        public void GetTypeAndNamespaceSyntax(string content, out TypeDeclarationSyntax type, out NamespaceDeclarationSyntax @namespace)
        {
            type = null;
            @namespace = null;

            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            GetTypeAndNamespaceSyntax(syntaxTree, out type, out @namespace);
        }

        public void GetTypeAndNamespaceSyntax(CompilationUnitSyntax ast, out TypeDeclarationSyntax type, out NamespaceDeclarationSyntax @namespace)
        {
            type = null;
            @namespace = null;

            if (ast == null)
            {
                return;
            }

            type = GetAvailableTypeSyntax(ast);
            @namespace = GetAvailableNamespaceSyntax(ast);
        }

        public void GetConstructors(TypeDeclarationSyntax typeDeclaration, out List<ConstructorDeclarationSyntax> constructors)
        {
            constructors = new List<ConstructorDeclarationSyntax>();

            if (typeDeclaration is null)
            {
                return;
            }

            constructors = typeDeclaration.Members.OfType<ConstructorDeclarationSyntax>().ToList();
        }

        TypeDeclarationSyntax GetAvailableTypeSyntax(CompilationUnitSyntax compilationUnitSyntax)
        {
            if (compilationUnitSyntax == null || !compilationUnitSyntax.Members.Any())
            {
                return null;
            }

            var typeSyntax = compilationUnitSyntax.Members.OfType<TypeDeclarationSyntax>().FirstOrDefault();
            if (typeSyntax != null)
            {
                return typeSyntax;
            }

            var ns = compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (ns == null)
            {
                return default;
            }

            return ns.Members.OfType<TypeDeclarationSyntax>().FirstOrDefault();
        }

        NamespaceDeclarationSyntax GetAvailableNamespaceSyntax(CompilationUnitSyntax compilationUnitSyntax)
        {
            if (compilationUnitSyntax == null || !compilationUnitSyntax.Members.Any())
            {
                return null;
            }

            return compilationUnitSyntax.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        }

        public CompilationUnitSyntax GenerateSyntax(Project project, string className, string @namespace, CompilationUnitSyntax ast)
        {
            GetTypeAndNamespaceSyntax(ast, out var type, out var namespaceSyntax);
            GetConstructors(type, out var constructors);

            var usingSyntax = UsingDirectiveGenerator.GenerateSyntax("System");

            var rootNamespaceSyntax = NamespaceDeclarationGenerator.GenerateSyntax(@namespace);

            var code = ast.ToString();

            if (namespaceSyntax != null)
            {
                var contents = RewriteCodeContent(className, @namespace, type, namespaceSyntax, constructors, code);

                return SyntaxFactory.ParseCompilationUnit(contents);
            }
            else
            {
                var contents = RewriteCodeContent(className, @namespace, type, namespaceSyntax, constructors, code);

                var typeDeclaration = SyntaxFactory.ParseCompilationUnit(contents);

                rootNamespaceSyntax = rootNamespaceSyntax.WithMembers(typeDeclaration.Members);

                var unit = SyntaxFactory.CompilationUnit().AddUsings(usingSyntax).AddMembers(rootNamespaceSyntax);

                var options = FormattingPolicyService.GetFormattingPolicy(project);

                unit = (CompilationUnitSyntax)Microsoft.CodeAnalysis.Formatting.Formatter.Format(unit, project.Solution.Workspace, options.OptionSet);

                return unit;
            }
        }

        string RewriteCodeContent(string className, string namespaceName, TypeDeclarationSyntax type, NamespaceDeclarationSyntax @namespace, List<ConstructorDeclarationSyntax> constructors, string code)
        {
            var contents = code;

            if (constructors != null && constructors.Any())
            {
                constructors = constructors.OrderByDescending(c => c.Span.Start).ToList();

                foreach (var constructor in constructors)
                {
                    contents = contents.Remove(constructor.Identifier.Span.Start, constructor.Identifier.Span.Length).Insert(constructor.Identifier.Span.Start, className);
                }
            }

            contents = contents.Remove(type.Identifier.Span.Start, type.Identifier.Span.Length).Insert(type.Identifier.Span.Start, className);

            if (@namespace != null)
            {
                contents = contents.Remove(@namespace.Name.Span.Start, @namespace.Name.Span.Length).Insert(@namespace.Name.Span.Start, namespaceName);
            }

            return contents;
        }

        public CompilationUnitSyntax GenerateSyntax(Project project, string className, string @namespace, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            return GenerateSyntax(project, className, @namespace, syntaxTree);
        }

        public string GenerateCode(Project project, string className, string @namespace, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            return GenerateCode(project, className, @namespace, syntaxTree);
        }

        public string GenerateCode(Project project, string className, string @namespace, CompilationUnitSyntax ast)
        {
            return GenerateSyntax(project, className, @namespace, ast)?.ToString() ?? string.Empty;
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string className, string folderPath, string @namespace, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<IWorkUnit>();
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            return Generate(project, className, folderPath, @namespace, syntaxTree);
        }

        public IReadOnlyList<IWorkUnit> Generate(Project project, string className, string folderPath, string @namespace, CompilationUnitSyntax ast)
        {
            if (ast is null)
            {
                throw new System.ArgumentNullException(nameof(ast));
            }

            var code = GenerateCode(project, className, @namespace, ast);

            var filePath = className;

            if (!filePath.EndsWith(".cs"))
            {
                filePath += ".cs";
            }

            if (!string.IsNullOrEmpty(folderPath))
            {
                filePath = Path.Combine(folderPath, filePath);
            }

            return new CreateProjectFileWorkUnit()
            {
                TargetProject = project,
                FilePath = filePath,
                FileContent = code,
            }.AsList();
        }

        public IReadOnlyList<IWorkUnit> Generate(CreateClassFromClipboardOptions options, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<IWorkUnit>();
            }

            var syntaxTree = SyntaxFactory.ParseCompilationUnit(content);

            return Generate(options, syntaxTree);
        }

        public IReadOnlyList<IWorkUnit> Generate(CreateClassFromClipboardOptions options, CompilationUnitSyntax ast)
        {
            if (options is null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }

            if (ast is null)
            {
                throw new System.ArgumentNullException(nameof(ast));
            }

            GetTypeAndNamespaceSyntax(ast, out var type, out var @namespaceSyntax);

            var @namespace = NamespaceDeclarationGenerator.GetNamespaceFor(options.Project, options.FolderPath);
            switch (options.NamespaceMode)
            {
                case NamespaceMode.Preserve:
                    @namespace = namespaceSyntax.Name.ToString();
                    break;
                case NamespaceMode.Custom:
                    @namespace = options.CustomNamespace;
                    break;
                case NamespaceMode.Automatic:
                    break;
            }

            return Generate(options.Project, options.Name, options.FolderPath, @namespace, ast);
        }
    }
}