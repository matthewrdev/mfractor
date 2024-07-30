using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Utilities.SyntaxVisitors
{
    public class UsingDirectiveVisitor : CSharpSyntaxVisitor
    {
        public List<UsingDirectiveSyntax> Usings = new List<UsingDirectiveSyntax>();

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            Usings.Add(node);
        }

        public override void VisitCompilationUnit(CompilationUnitSyntax node)
        {
            foreach (var use in node.Usings)
            {
                VisitUsingDirective(use);
            }
        }
    }
}
