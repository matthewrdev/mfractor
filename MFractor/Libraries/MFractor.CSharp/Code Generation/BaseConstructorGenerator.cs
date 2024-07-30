using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBaseConstructorGenerator))]
    class BaseConstructorGenerator : CSharpCodeGenerator, IBaseConstructorGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.base_constructor";

        public override string Name => "Create Base Constructor";

        public override string Documentation => "Generates a class constructor that routes a series of constructor parameters into a `base(...)` constructor.";

        [Import]
        public IConstructorGenerator ConstructorGenerator { get; set; }

        public ConstructorDeclarationSyntax GenerateSyntax(IMethodSymbol constructor, string className)
        {
            var argsList = new List<ArgumentSyntax>();

            var parameters = new List<Tuple<ITypeSymbol, string>>();

            foreach (var arg in constructor.Parameters) {
                
                argsList.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(arg.Name)));

                parameters.Add(new Tuple<ITypeSymbol, string>(arg.Type, arg.Name));
            }

            var constructorSyntax = ConstructorGenerator.GenerateSyntax(parameters, className);

            constructorSyntax = constructorSyntax.WithInitializer(
                        SyntaxFactory.ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            SyntaxFactory.ArgumentList().AddArguments(argsList.ToArray())));

            return constructorSyntax;
        }
    }
}
