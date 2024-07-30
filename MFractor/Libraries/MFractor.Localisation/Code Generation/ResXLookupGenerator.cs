using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.Localisation.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IResXLookupGenerator))]
    class ResXLookupGenerator : CodeGenerator, IResXLookupGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.resx.generate_csharp_resx_lookup";

        public override string Name => "Generate C# ResX Resource Lookup";

        public override string Documentation => "Generates C# resource lookup such as `MyApp.Resources.Resources.MyLabel`";

        public override string[] Languages { get; } = new string[] { "C#" };

        public SyntaxNode Generate(INamedTypeSymbol sourceType, string propertyName)
        {
            return Generate(sourceType.ToString(), propertyName);
        }

        public SyntaxNode Generate(string resourceSymbol, string propertyName)
		{
            var typeSyntax = SyntaxFactory.ParseTypeName(resourceSymbol);

			return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
														typeSyntax,
														SyntaxFactory.IdentifierName(propertyName));
        }
    }
}
