using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Logging;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace MFractor.CSharp.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImplementationFinder))]
    class ImplementationFinder : IImplementationFinder
    {
        readonly ILogger log = Logger.Create();

        public INamedTypeSymbol GetType(SemanticModel model, SyntaxNode syntax)
        {
            var type = GetType(model, syntax, out var needsMemberSymbolResolve);

            if (type == null || needsMemberSymbolResolve)
            {
                var member = GetMemberSymbol(model, syntax, out type);
            }

            if (type == null)
            {
                return null;
            }

            return type;
        }

        INamedTypeSymbol GetType(SemanticModel model, SyntaxNode syntax, out bool needsMemberResolve)
        {
            INamedTypeSymbol type = null;

            needsMemberResolve = false;
            if (syntax is InterfaceDeclarationSyntax typeSyntax)
            {
                type = model.GetDeclaredSymbol(typeSyntax) as INamedTypeSymbol;
            }
            else if (syntax is BaseTypeSyntax baseTypeSyntax)
            {
                type = model.GetTypeInfo(baseTypeSyntax.Type).Type as INamedTypeSymbol;
            }
            else if (syntax is IdentifierNameSyntax identifierSyntax)
            {
                if (!(syntax.Parent is MemberAccessExpressionSyntax))
                {
                    type = model.GetTypeInfo(identifierSyntax).Type as INamedTypeSymbol;
                }
            }
            else if (syntax is TypeConstraintSyntax typeConstraintSyntax)
            {
                type = model.GetTypeInfo(typeConstraintSyntax.Type).Type as INamedTypeSymbol;
            }
            else if (syntax is ClassDeclarationSyntax classSyntax)
            {
                type = model.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
            }

            if (type == null)
            {
                needsMemberResolve = true;
            }

            if (type == null)
            {
                return null;
            }

            return type;
        }

        public ISymbol GetMemberSymbol(SemanticModel model, SyntaxNode syntax, out INamedTypeSymbol type)
        {
            type = null;
            ISymbol symbol = null;

            if (syntax is MethodDeclarationSyntax
                || syntax is BasePropertyDeclarationSyntax)
            {
                symbol = model.GetDeclaredSymbol(syntax);
            }
            else if (syntax is ExpressionSyntax)
            {
                symbol = model.GetSymbolInfo(syntax).Symbol;
            }
            else if (syntax is NameSyntax nameSyntax)
            {
                var parent = nameSyntax.Parent;

                symbol = GetMemberSymbol(model, parent, out type);
            } 

            if (symbol != null)
            {
                type = SymbolHelper.GetMemberContainingType(symbol);
            }

            return symbol;
        }

        public IEnumerable<ISymbol> FindImplementations(Project project, Compilation compilation, SemanticModel model, SyntaxNode syntax)
        {
            var type = GetType(model, syntax, out var needsMemberSymbolResolve);

            ISymbol member = null;
            if (needsMemberSymbolResolve)
            {
                member = GetMemberSymbol(model, syntax, out var temp);

                if (member != null)
                {
                    type = temp;
                }
            }

            var solution = project.Solution;

            var results = new List<INamedTypeSymbol>();

            try
            {
                if (type.TypeKind == TypeKind.Interface)
                {
                    results = SymbolFinder.FindImplementationsAsync(type, solution).Result.Cast<INamedTypeSymbol>().ToList();
                }
                else
                {
                    results = SymbolFinder.FindDerivedClassesAsync(type, solution).Result.ToList();
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            if (!results.Any())
            {
                results.Add(type);
            }

            if (member == null)
            {
                return results;
            }

            var members = new List<ISymbol>();

            try
            {
                if (type.TypeKind == TypeKind.Interface)
                {
                    members = SymbolFinder.FindImplementationsAsync(member, solution).Result.ToList();
                }
                else
                {
                    members = SymbolFinder.FindOverridesAsync(member, solution).Result.ToList();
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            if (!members.Any())
            {
                members.Add(member);
            }

            return members;
        }
    }
}