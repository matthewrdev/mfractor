using System;
using MFractor.Code.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IAttributeUsageAnnotationGenerator : ICodeGenerator
    {
        AttributeListSyntax GenerateSyntax(AttributeTargets usageTarget);
    }
}
