using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IEventHandlerMethodGenerator))]
    class EventHandlerMethodGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IEventHandlerMethodGenerator
    {
        public override string Documentation => "Generates a method that's compatible for registration with an `EventHandler<EventArgs>` as a callback.";

        public override string Identifier => "com.mfractor.code_gen.csharp.event_handler_method";

        public override string Name => "Generate Event Handler Method";

        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the new method.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The fully qualified type of the event arguments.")]
        [CodeSnippetResource("Resources/Snippets/EventHandlerMethod.txt")]
        [ExportProperty("What is the code snippet to use when creating the new event handler method declaration?")]
        public ICodeSnippet Snippet
        {
            get;
            set;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(string eventName,
                                                                   string eventArgsTypeName)
        {
            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, eventArgsTypeName);
            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, eventName);

            return Snippet.AsMembersList();
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateSyntax(IEventSymbol eventSymbol, 
                                                                   string eventName)
        {
            var namedEventType = eventSymbol.Type as INamedTypeSymbol;

            var eventTypeArg = namedEventType.TypeArguments.Length > 0 ? namedEventType.TypeArguments.First().ToString() : "System.EventArgs";

            return GenerateSyntax(eventName, eventTypeArg);
        }
    }
}
