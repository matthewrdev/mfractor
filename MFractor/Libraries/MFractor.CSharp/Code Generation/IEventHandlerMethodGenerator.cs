using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    public interface IEventHandlerMethodGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }


        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName,
                                                            string eventArgsTypeName);

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(IEventSymbol eventSymbol, 
                                                            string eventName);
    }
}
