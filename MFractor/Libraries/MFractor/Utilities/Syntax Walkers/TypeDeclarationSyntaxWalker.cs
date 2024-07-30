using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Utilities.SyntaxWalkers
{
    public class TypeDeclarationSyntaxWalker : SyntaxWalker
    {
        public readonly List<TypeDeclarationSyntax> Types = new List<TypeDeclarationSyntax>();

        public override void Visit(SyntaxNode node)
        {
            if (node is TypeDeclarationSyntax type)
            {
                Types.Add(type);
            }
            else
            {
                base.Visit(node);
            }
        }
    }
}
