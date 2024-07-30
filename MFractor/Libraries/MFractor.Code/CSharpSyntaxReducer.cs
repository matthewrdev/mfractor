using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Ide;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Code
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICSharpSyntaxReducer))]
    [Export(typeof(IFileCreationPostProcessor))]
    class CSharpSyntaxReducer : ICSharpSyntaxReducer
    {
        readonly IUsingDirectiveGenerator usingDirectiveGenerator;

        public string ContentType => "CSharp";

        [ImportingConstructor]
        public CSharpSyntaxReducer(IUsingDirectiveGenerator usingDirectiveGenerator)
        {
            this.usingDirectiveGenerator = usingDirectiveGenerator;
        }

        public SyntaxNode Reduce(SyntaxNode syntax,  
                                 SemanticModel model,
                                 List<string> knownTypes,
                                 ref List<UsingDirectiveSyntax> usings)
        {
            if (syntax is null)
            {
                return null;
            }

            if (syntax is TypeSyntax typeSyntax)
            {
                return ReduceType(typeSyntax, model, knownTypes, ref usings);
            }

            var reducedChildren = new Dictionary<SyntaxNode, SyntaxNode>();

            var reduced = syntax;

            foreach (var child in syntax.ChildNodes())
            {
                var reducedChild = child;
                if (child is TypeSyntax nameSyntax)
                {
                    reducedChild = ReduceType(nameSyntax, model, knownTypes, ref usings);
                }
                else
                {
                    reducedChild = Reduce(child, model, knownTypes, ref usings);
                }
                reducedChildren.Add(child, reducedChild);
            }

            reduced = reduced.ReplaceNodes(reducedChildren.Keys, (SyntaxNode arg1, SyntaxNode arg2) =>
            {
                if (reducedChildren.ContainsKey(arg1))
                {
                    return reducedChildren[arg1];
                }
                return arg1;
            });

            return reduced;
        }

        TypeSyntax ReduceType(TypeSyntax nameSyntax, 
                              SemanticModel model, 
                              List<string> knownTypes,
                              ref List<UsingDirectiveSyntax> usings)
        {
            var typeName = "";
            var namespaceName = "";
            if (nameSyntax is QualifiedNameSyntax qualifiedName 
                && ShouldReduce(qualifiedName))
            {
                var right = qualifiedName.Right;
                if (right is GenericNameSyntax genericName)
                {
                    var args = SyntaxFactory.TypeArgumentList();

                    foreach (var arg in genericName.TypeArgumentList.Arguments)
                    {
                        var reduced = ReduceType(arg, model, knownTypes, ref usings);
                        args = args.AddArguments(reduced);
                    }

                    typeName = SyntaxFactory.GenericName(SyntaxFactory.Identifier(genericName.Identifier.Text), args).NormalizeWhitespace().ToString();
                }
                else
                {
                    typeName = right.ToString();
                }

                var performNaiveReduction = true;
                if (model != null)
                {
                    try
                    {
                        var symbol = model.GetTypeInfo(qualifiedName);
                        if (symbol.Type?.ContainingNamespace != null)
                        {
                            namespaceName = symbol.Type.ContainingNamespace.ToString();
                            performNaiveReduction = false;
                        }
                    }
                    catch
                    {
                    }
                }

                if (performNaiveReduction)
                {
                    namespaceName = qualifiedName.Left.ToString();
                }
            }
            else
            {
                return nameSyntax;
            }

            if (knownTypes.Any(kt => kt.Split('.').Last() == typeName && kt != (namespaceName + "." + typeName)))
            {
                return nameSyntax;
            }

            var reducedChild = SyntaxFactory.IdentifierName(typeName).WithLeadingTrivia(nameSyntax.GetLeadingTrivia()).WithTrailingTrivia(nameSyntax.GetTrailingTrivia());
            usings.Add(usingDirectiveGenerator.GenerateSyntax(namespaceName));
            knownTypes.Add(namespaceName + "." + typeName);

            return reducedChild;
        }

        bool ShouldReduce(QualifiedNameSyntax qualifiedName)
        {
            var parent = qualifiedName.Parent;

            while (parent != null)
            {
                if (parent is UsingDirectiveSyntax
                    || parent is NamespaceDeclarationSyntax)
                {
                    return false;
                }

                if (parent is MemberDeclarationSyntax
                    || parent is AttributeSyntax
                    || parent is AttributeListSyntax )
                {
                    return true;
                }

                parent = parent.Parent;
            }

            return true;
        }

        public string Reduce(string code, Project project)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (string.IsNullOrEmpty(code))
            {
                return code;
            }

            var documentGuid = Guid.NewGuid().ToString().Replace("-", "") + ".cs";

            var syntax = SyntaxFactory.ParseCompilationUnit(code);

            var document = project.AddDocument(documentGuid, syntax);

            var root = document.GetSyntaxRootAsync().Result;

            var usings = new List<UsingDirectiveSyntax>();

            var existingUsingsSpan = new TextSpan(0, 0);
            if (syntax.Usings.Any())
            {
                existingUsingsSpan = syntax.Usings.Span;
            }

            var model = document.GetSemanticModelAsync().Result;

            var reduced = Reduce(root, model, new List<string>(), ref usings);

            usings = DeduplicateAndSortUsings(syntax.Usings.ToList(), usings).ToList();

            var reducedCode = reduced.ToString();

            if (usings != null && usings.Any())
            {
                var usingsCode = string.Join(Environment.NewLine, usings.Select(an => an.ToString())) + Environment.NewLine;

                if (existingUsingsSpan.Start != 0)
                {
                    usingsCode = Environment.NewLine + usingsCode;
                }

                if (existingUsingsSpan.Length > 0)
                {
                    reducedCode = reducedCode.Remove(existingUsingsSpan.Start, existingUsingsSpan.Length);
                }

                reducedCode = reducedCode.Insert(existingUsingsSpan.Start, usingsCode);
            }

            return reducedCode;
        }

        bool IsSystemUsingDirectory(UsingDirectiveSyntax use)
        {
            return use.Name.ToString() == "System" || use.Name.ToString().StartsWith("System", StringComparison.Ordinal);
        }

        public IReadOnlyList<UsingDirectiveSyntax> DeduplicateAndSortUsings(IReadOnlyList<UsingDirectiveSyntax> existing,
                                                                            IReadOnlyList<UsingDirectiveSyntax> additional)
        {

            var usings = new Dictionary<string, UsingDirectiveSyntax>();

            if (additional != null)
            {
                foreach (var use in additional)
                {
                    if (!usings.ContainsKey(use.ToString()))
                    {
                        usings[use.ToString()] = use.NormalizeWhitespace();
                    }
                }
            }

            if (existing != null)
            {
                foreach (var use in existing)
                {
                    if (!usings.ContainsKey(use.ToString()))
                    {
                        usings.Add(use.ToString(), use);
                    }
                }
            }

            var systemUsings = usings.Values.Where(IsSystemUsingDirectory).OrderBy((arg) => arg.ToString()).ToList();

            var system = systemUsings.FirstOrDefault(use => use.Name.ToString() == "System");

            if (system != null)
            {
                systemUsings.Remove(system);
                systemUsings.Insert(0, system);
            }

            var otherUsings = usings.Values.Except(systemUsings).OrderBy((arg) => arg.ToString());

            var sorted = new List<UsingDirectiveSyntax>();

            sorted.AddRange(systemUsings);
            sorted.AddRange(otherUsings);

            var lastUsing = existing?.LastOrDefault();

            if (lastUsing != null)
            {
                var trailingTrivia = lastUsing.GetTrailingTrivia();
                var last = sorted.Last();
                sorted.Remove(last);
                last = last.WithTrailingTrivia(trailingTrivia);
                sorted.Add(last);
            }

            return sorted;
        }

        public bool CanPostProcess(FileInfo fileInfo, Project project)
        {
            if (fileInfo is null
                || project is null)
            {
                return false;
            }

            return string.Equals(fileInfo.Extension, ".cs", StringComparison.OrdinalIgnoreCase);
        }

        public string PostProcess(string content, FileInfo fileInfo, Project project)
        {
            if (string.IsNullOrEmpty(content)
                || fileInfo is null
                || project is null)
            {
                return content;
            }

            return Reduce(content, project);
        }
    }
}