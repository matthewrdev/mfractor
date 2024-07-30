using System;
using System.ComponentModel.Composition;
using MFractor.Configuration.Attributes;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMemberFieldGenerator))]
    class MemberFieldGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IMemberFieldGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.field_declaration";

        public override string Name => "Member Field Generator";

        public override string Documentation => "Generates member field declarations with an optional value initialisation";

        [ExportProperty("When creating a backing field for a property, should the field have an underscore appended to the start of the field name?")]
        public bool UnderscoreOnBackingField
        {
            get;
            set;
        } = false;

        [Import]
        public IMemberInitialiserGenerator MemberInitialiserGenerator
        {
            get;
            set;
        }

        public string CreateFieldName(string candidateFieldName)
        { 
            return UnderscoreOnBackingField ? "_" + StringExtensions.FirstCharToLower(candidateFieldName) : StringExtensions.FirstCharToLower(candidateFieldName);
        }

        public FieldDeclarationSyntax GenerateSyntax(ITypeSymbol type, string name, string value)
		{
            var fieldName = CreateFieldName(name);

			var typeSyntax = SyntaxFactory.ParseTypeName(type.ToString());


			var variableDeclaration = SyntaxFactory.VariableDeclarator(
				SyntaxFactory.Identifier(fieldName));

			if (string.IsNullOrEmpty(value) == false)
			{
                variableDeclaration = variableDeclaration.WithInitializer(MemberInitialiserGenerator.GenerateSyntax(type, value));
			}

			var fieldSyntax = SyntaxFactory.FieldDeclaration(
								SyntaxFactory.VariableDeclaration(typeSyntax)
								.WithVariables(
										SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(variableDeclaration
										)))
									.WithModifiers(
										SyntaxFactory.TokenList(
											SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
													   .NormalizeWhitespace();

            return fieldSyntax;
        }
    }
}
