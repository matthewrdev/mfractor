using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITypeSyntaxSimplifier))]
    class TypeSyntaxSimplifier : ITypeSyntaxSimplifier
    {
        readonly Lazy<ICSharpSyntaxReducer> syntaxReducer;
        public ICSharpSyntaxReducer SyntaxReducer => syntaxReducer.Value;

        [ImportingConstructor]
        public TypeSyntaxSimplifier(Lazy<ICSharpSyntaxReducer> syntaxReducer)
        {
            this.syntaxReducer = syntaxReducer;
        }

        public IReadOnlyList<UsingDirectiveSyntax> GetDeduplicatedUsings(SyntaxTree syntaxTree, IReadOnlyList<UsingDirectiveSyntax> usings)
        {
            var result = new List<UsingDirectiveSyntax>();

            var root = syntaxTree.GetRoot() as CompilationUnitSyntax;

            if (root == null)
            {
                return null;
            }

            var currentUsings = root.Usings.ToList();

            foreach (var @using in usings)
            {
                var useString = @using.NormalizeWhitespace().ToString();
                var match = currentUsings.FirstOrDefault(u => u.NormalizeWhitespace().ToString() == useString);
                if (match == null)
                {
                    result.Add(@using);
                }
            }

            return result;
        }

        public IReadOnlyList<UsingDirectiveSyntax> GetReducedTypeUsings(TypeSyntax typeSyntax, Compilation compilation, out SyntaxNode reducedSyntaxNode, out QualifiedNameSyntax qualifiedNameSyntax)
        {
            reducedSyntaxNode = null;
            qualifiedNameSyntax = null;

            var syntaxTree = typeSyntax.SyntaxTree;

            var model = compilation.GetSemanticModel(syntaxTree);

            SyntaxNode syntax = typeSyntax;
            while (syntax != null)
            {
                if (syntax is QualifiedNameSyntax qualified
                    && !(syntax.Parent is QualifiedNameSyntax))
                {
                    qualifiedNameSyntax = qualified;
                    break;
                }

                syntax = syntax.Parent;
            }

            if (qualifiedNameSyntax == null)
            {
                return null;
            }

            var usings = new List<UsingDirectiveSyntax>();

            reducedSyntaxNode = SyntaxReducer.Reduce(qualifiedNameSyntax, model, new List<string>(), ref usings);

            return usings;
        }
    }
}