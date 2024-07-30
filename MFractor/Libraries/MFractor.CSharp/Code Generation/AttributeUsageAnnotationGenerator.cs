using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MFractor.Code.CodeGeneration.CSharp;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAttributeUsageAnnotationGenerator))]
    class AttributeUsageAnnotationGenerator : CSharpCodeGenerator, IAttributeUsageAnnotationGenerator
    {
        public override string Documentation => "Create an `[System.AttributeUsage()]` annotation that can can be attached to a class declaration that derives from `System.Attribute`.";

        public override string Identifier => "com.mfractor.code_gen.csharp.attribute_usage_annotation";

        public override string Name => "Create AttributeUsage Annotation";

        public AttributeListSyntax GenerateSyntax(AttributeTargets usageTarget)
        {
            return SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                    SyntaxFactory.Attribute(
                        SyntaxFactory.QualifiedName(
                            SyntaxFactory.IdentifierName("System"),
                            SyntaxFactory.IdentifierName("AttributeUsage")))
                    .WithArgumentList(
                        SyntaxFactory.AttributeArgumentList(

                            SyntaxFactory.SingletonSeparatedList<AttributeArgumentSyntax>(
                                SyntaxFactory.AttributeArgument(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName("System"),
                                            SyntaxFactory.IdentifierName("AttributeTargets")),
                                        SyntaxFactory.IdentifierName(usageTarget.ToString()))))))));
        }
    }
}
