using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEventHandlerDeclarationGenerator))]
    class EventHandlerDeclarationGenerator : CSharpCodeGenerator, IEventHandlerDeclarationGenerator
    {
        public override string Documentation => "Generates an `event EventHandler<EventArgs>` declaration that can be bound against by a callback method.";

        public override string Identifier => "com.mfractor.code_gen.csharp.event_handler_declaration";

        public override string Name => "Generate Event Handler Declaration";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new event handler.")]
        [CodeSnippetArgument("event_type", "The fully qualified type of the event handler.")]
        [CodeSnippetArgument("arguments_type", "The fully qualified type of the event arguments.")]
        [CodeSnippetResource("Resources/Snippets/EventHandlerDeclarationWithArgs.txt")]
        [ExportProperty("What is the code snippet to use when creating a new event handler that includes event arguments?")]
        public ICodeSnippet SnippetWithArgs { get; set; }

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new event handler.")]
        [CodeSnippetArgument("event_type", "The fully qualified type of the event handler.")]
        [CodeSnippetResource("Resources/Snippets/EventHandlerDeclaration.txt")]
        [ExportProperty("What is the code snippet to use when creating a new event handler?")]
        public ICodeSnippet Snippet { get; set; }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName, string eventHandlerType)
        {
            Snippet.SetArgumentValue("event_type", eventHandlerType);
            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, eventName);

            return Snippet.AsMembersList();
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName, string eventHandlerType, string eventArgsType)
        {
            SnippetWithArgs.SetArgumentValue("event_type", eventHandlerType);
            SnippetWithArgs.SetArgumentValue("arguments_type", eventArgsType ?? "System.EventArgs");
            SnippetWithArgs.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, eventName);

            return SnippetWithArgs.AsMembersList();
        }
    }
}
