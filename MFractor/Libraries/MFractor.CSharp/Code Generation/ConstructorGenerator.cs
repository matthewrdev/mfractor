using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConstructorGenerator))]
    class ConstructorGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IConstructorGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.constructor";

        public override string Name => "Create Constructor";

        public override string Documentation => "Generates a class constructor, optionally with a set of arguments.";

        [ExportProperty("Should all constructor arguments have their first letter forced to lower case?")]
        public bool ArgumentFirstLetterToLowerCase
        {
            get;
            set;
        } = true;

        public ConstructorDeclarationSyntax GenerateSyntax(List<Tuple<ITypeSymbol, string>> parameters, string className)
        {
            var parametersList = default(SeparatedSyntaxList<ParameterSyntax>);

            if (parameters != null && parameters.Any())
            {
                foreach (var arg in parameters)
                {
                    var type = SyntaxFactory.ParseTypeName(arg.Item1.ToString());

                    var name = arg.Item2;

                    if (ArgumentFirstLetterToLowerCase)
                    {
                        name = StringExtensions.FirstCharToLower(name);
                    }

                    parametersList = parametersList.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier(name)).WithType(type));
                }
            }

            var result = SyntaxFactory.ConstructorDeclaration(
                SyntaxFactory.Identifier(className))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                     SyntaxFactory.ParameterList().WithParameters(parametersList))
                .WithBody(SyntaxFactory.Block());

            return result;
        }
    }
}
