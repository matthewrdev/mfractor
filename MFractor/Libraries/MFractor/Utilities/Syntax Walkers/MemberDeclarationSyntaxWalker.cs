using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Utilities.SyntaxWalkers
{
    public class MemberDeclarationSyntaxWalker : SyntaxWalker
    {
        public readonly List<MemberDeclarationSyntax> Members = new List<MemberDeclarationSyntax>();

        public override void Visit(SyntaxNode node)
        {
            if (node is MemberDeclarationSyntax)
            {
                Members.Add(node as MemberDeclarationSyntax);
            }
            else
            {
                base.Visit(node);
            }
        }
    }
}
