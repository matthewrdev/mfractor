using MFractor.Code.Documents;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.CodeActions
{
    /// <summary>
    /// Code action execution filters for detecting and filtering <see cref="CSharpCodeAction"/> instances.
    /// <para/>
    /// These filters should be returned by <see cref="ICodeAction.Filter"/>.
    /// </summary>
    public static class CSharpCodeActionExecutionFilters
    {
        /// <summary>
        /// A filter that checks if a given document is a C# file.
        /// </summary>
        public static readonly DocumentExecutionFilter CSharpDocument = new DocumentExecutionFilter("C# Document", document => document is ParsedCSharpDocument, null);

        /// <summary>
        /// A filter that checks if a given document is a C# file and that the syntax is a C# syntax node.
        /// </summary>
        public static readonly DocumentExecutionFilter SyntaxNode = new DocumentExecutionFilter("C# Syntax", document => document is ParsedCSharpDocument, (syntax) => syntax is SyntaxNode);
    }
}
