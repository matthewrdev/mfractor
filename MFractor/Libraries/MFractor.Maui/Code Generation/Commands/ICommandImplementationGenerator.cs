using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.Commands
{
    public interface ICommandImplementationGenerator : ICodeGenerator
    {
        ICodeSnippet Snippet { get; set; }

        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string commandName, IXamlPlatform platform);
        IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string commandName, string commandType);
	}
}
