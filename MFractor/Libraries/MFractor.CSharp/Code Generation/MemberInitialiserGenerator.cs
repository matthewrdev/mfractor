using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.CodeGeneration;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMemberInitialiserGenerator))]
    class MemberInitialiserGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IMemberInitialiserGenerator
    {
        [ExportProperty("Should the code generator always attempt to assign members with a string literal?")]
        public bool ForceStringLiteral { get; set; } = false;

        public override string Documentation => "Generates an initilisation expression for a class/struct member (such as a property or field).";

        public override string Identifier => "com.mfractor.code_gen.csharp.member_initialiser_expression";

        public override string Name => "Generate Member Initialiser";

        public EqualsValueClauseSyntax GenerateSyntax(ITypeSymbol initialiserType, string initialiserValue)
        {
            ExpressionSyntax innerValueNode = null;

            var namedType = (INamedTypeSymbol)initialiserType;

            var members = initialiserType.GetMembers(initialiserValue);

            var staticMember = members.Where(m => m is IFieldSymbol || m is IPropertySymbol).FirstOrDefault(symbol => symbol.IsStatic);

            if (staticMember != null)
            {
                innerValueNode = SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(initialiserType.ToString()),
                        SyntaxFactory.IdentifierName(initialiserValue));

            }
            else if (ForceStringLiteral)
            {
                innerValueNode = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(initialiserValue));
            }
            else if (initialiserType.SpecialType == SpecialType.System_String
            || (initialiserType.IsValueType && namedType.TypeKind != TypeKind.Struct && initialiserType.SpecialType == SpecialType.None))
            {
                innerValueNode = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(initialiserValue));
            }
            else if (initialiserType.IsValueType && (namedType.TypeKind != TypeKind.Struct || namedType.SpecialType != SpecialType.None))
            {
                switch (initialiserType.SpecialType)
                {
                    case SpecialType.System_Boolean:
                        {
                            var literal = bool.Parse(initialiserValue) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression;
                            innerValueNode = SyntaxFactory.LiteralExpression(literal);
                        }
                        break;
                    case SpecialType.System_Char:
                        {
                            innerValueNode = SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SyntaxFactory.Literal(char.Parse(initialiserValue)));
                        }
                        break;
                    default: // Numerical literal.
                        {
                            innerValueNode = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(double.Parse(initialiserValue)));
                        }
                        break;
                }
            }
            else
            {

                var constructors = namedType.Constructors;

                if (constructors.Length == 1) // HACK: Only default constructor, just do a string assignment :/
                {
                    innerValueNode = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(initialiserValue));
                }
                else
                {

                    var constructParams = initialiserValue.Split(',').Length;

                    var candidates = constructors.Where(c => c.Parameters.Length == constructParams);

                    string value = initialiserValue;
                    if (!candidates.Any())
                    {
                        var singleParamConstruct = candidates.Where(c => c.Parameters.Length == 1);
                        if (singleParamConstruct.Any())
                        {
                            foreach (var spc in singleParamConstruct)
                            {
                                if (spc.Parameters.First().Type.SpecialType == SpecialType.System_String)
                                {
                                    value = "\"" + initialiserValue + "\"";
                                    break;
                                }
                            }
                        }
                    }


                    string code = code = $"new {initialiserType.ToString()}({value})";

                    innerValueNode = SyntaxFactory.ParseExpression(code);
                }
            }

            return SyntaxFactory.EqualsValueClause(innerValueNode);
        }
    }
}
