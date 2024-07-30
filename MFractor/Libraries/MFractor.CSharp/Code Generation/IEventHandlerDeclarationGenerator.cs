using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IEventHandlerDeclarationGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName, string eventHandlerType);
        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName, string eventHandlerType, string eventArgsType);
    }
}
