using System;
using MFractor.Code.CodeGeneration;
using MFractor.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IMemberAssignmentGenerator : ICodeGenerator
    {
        bool IncludeThisForMembers { get; set; }

        AssignmentExpressionSyntax GenerateSyntax(string assignee, string value, bool isMember);
    }
}
