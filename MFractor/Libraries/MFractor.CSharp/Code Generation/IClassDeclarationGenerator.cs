using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IClassDeclarationGenerator : ICodeGenerator
    {
        bool ImplementBaseConstructors { get; set; }

        IMemberFieldGenerator MemberFieldGenerator { get; set; }

        IInstancePropertyGenerator InstancePropertyGenerator { get; set; }

        IMethodGenerator MethodGenerator { get; set; }

        IBaseConstructorGenerator BaseConstructorGenerator { get; set; }

        ClassDeclarationSyntax GenerateSyntax(string className,
                                              INamedTypeSymbol baseType,
                                              IEnumerable<MemberDeclaration> members);

        ClassDeclarationSyntax GenerateSyntax(string className,
                                              string baseType,
                                              IEnumerable<MemberDeclaration> members);

        IEnumerable<MemberDeclarationSyntax> GenerateMembers(IEnumerable<MemberDeclaration> members, IEnumerable<MemberDeclaration> membersToExclude, INamedTypeSymbol baseType);

        IEnumerable<MemberDeclarationSyntax> GenerateMembers(IEnumerable<ISymbol> members, IEnumerable<MemberDeclaration> membersToExclude, out List<MemberDeclaration> excludedMembers);

        ClassDeclarationSyntax GenerateSyntax(string className, string baseTypeName, INamedTypeSymbol baseTypeSymbol, IEnumerable<MemberDeclaration> members);
    }
}
