using System.ComponentModel.Composition;
using MFractor.Code.CodeGeneration.CSharp;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IExceptionHandlerGenerator))]
    class ExceptionHandlerGenerator : CSharpCodeGenerator, IExceptionHandlerGenerator
    {
        [ExportProperty("The code snippet to handle an exception.")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Name, "The name of the exception variable")]
        [CodeSnippetArgument(ReservedCodeSnippetArgumentName.Type, "The type of the exception variable")]
        [CodeSnippetDefaultValue("// TODO: Handle exception.", "The code snippet to handle an exception")]
        public ICodeSnippet Snippet { get; set; }

        public override string Identifier => "com.mfractor.code_gen.csharp.exception_handler";

        public override string Name => "Exception Handler Generator";

        public override string Documentation => "A code generator that create one or more statements to handle an exception";

        public StatementSyntax GenerateSyntax(string exceptionName, INamedTypeSymbol exceptionTypeSymbol)
        {
            return GenerateSyntax(exceptionName, exceptionTypeSymbol?.ToString());
        }

        public StatementSyntax GenerateSyntax(string exceptionName, string exceptionType)
        {
            Apply(exceptionName, exceptionType);

            return Snippet.AsStatement();
        }

        public string GenerateText(string exceptionName, INamedTypeSymbol exceptionTypeSymbol)
        {
            return GenerateText(exceptionName, exceptionTypeSymbol?.ToString());
        }

        public string GenerateText(string exceptionName, string exceptionType)
        {
            Apply(exceptionName, exceptionType);

            return Snippet.ToString();
        }

        void Apply(string exceptionName, string exceptionType)
        {
            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Name, exceptionName);
            Snippet.SetArgumentValue(ReservedCodeSnippetArgumentName.Type, exceptionType);
        }
    }
}