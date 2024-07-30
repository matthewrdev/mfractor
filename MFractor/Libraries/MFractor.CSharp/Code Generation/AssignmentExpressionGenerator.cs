using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMemberAssignmentGenerator))]
    class AssignmentExpressionGenerator : CSharpCodeGenerator, IMemberAssignmentGenerator
	{
		public override string Name => "Generate Assignment Expression";

        public override string Documentation => "Generates an assignment expresssion; eg `this.myField = value;`";

        public override string Identifier => "com.mfractor.code_gen.csharp.assignment_expression";

        [ExportProperty("When assigning to a class member, should a `this.` be added to the variable being assigned?")]
        public bool IncludeThisForMembers { get; set; } = false;

        public AssignmentExpressionSyntax GenerateSyntax(string assignee, string value, bool isMember)
        {
            var requiresThis = isMember && IncludeThisForMembers;
            if (isMember && assignee == value)
            {
                requiresThis = true;
            }

            ExpressionSyntax left = SyntaxFactory.IdentifierName(assignee);

            if (requiresThis)
            {
                left = SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ThisExpression(),
                                SyntaxFactory.IdentifierName(assignee));
            }

            return SyntaxFactory.AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                left,
                                SyntaxFactory.IdentifierName(value)); 

        }
    }
}
