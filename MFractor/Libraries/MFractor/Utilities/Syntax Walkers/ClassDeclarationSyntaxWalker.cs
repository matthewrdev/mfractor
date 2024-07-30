using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Utilities.SyntaxWalkers
{
    public class ClassDeclarationSyntaxWalker : SyntaxWalker
    {
        public readonly List<ClassDeclarationSyntax> Classes = new List<ClassDeclarationSyntax>();

        public override void Visit(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax)
            {
                Classes.Add(node as ClassDeclarationSyntax);
            }
            else
            {
                base.Visit(node);
            }
        }
    }
}
