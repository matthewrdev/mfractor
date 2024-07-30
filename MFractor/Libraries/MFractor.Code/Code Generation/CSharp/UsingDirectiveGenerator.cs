using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Code.CodeGeneration.CSharp
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IUsingDirectiveGenerator))]
    class UsingDirectiveGenerator : CSharpCodeGenerator, IUsingDirectiveGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.using_directive";

        public override string Name => "Using Directive Generator";

        public override string Documentation => "Generates a new using directive for the provided namespace";

        public UsingDirectiveSyntax GenerateSyntax(INamespaceSymbol namespaceSymbol)
        {
            return GenerateSyntax(namespaceSymbol.ToString());
        }

        public UsingDirectiveSyntax GenerateSyntax(string namespaceName)
        {
            return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName.Replace(" ", "")))
                                .NormalizeWhitespace();
        }
    }
}
