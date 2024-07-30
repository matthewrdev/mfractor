using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeGeneration.Commands
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICommandImplementationGenerator))]
    class CommandImplementationGenerator : CodeGenerator, ICommandImplementationGenerator
    {
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new command.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the new command")]
        [CodeSnippetResource("Resources/Snippets/Command.txt")]
        [ExportProperty("What is the code snippet to use when creating the command stub? If not set, this code generator will default to generating a stub `.Command` implementation")]
        public ICodeSnippet Snippet { get; set; } = null;

        public override string[] Languages { get; } = new string[] { "C#" };

        public override string Documentation => "Generates a boilerplate implementation of ICommand using an inline Command.";

        public override string Identifier => "com.mfractor.code_gen.xaml.csharp.command_implementation";

        public override string Name => "Generate ICommand Implementation";

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string commandName, IXamlPlatform platform)
        {
            return GenerateSyntax(commandName, platform.Command.MetaType);
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string commandName, string commandType)
        {
            return Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, commandName)
                          .SetArgumentValue(ReservedCodeSnippetArgumentName.Type, commandType)
                          .AsMembersList();
        }
    }
}
