using System.Collections.Generic;
using System.Linq;
using MFractor.IOC;
using MFractor.Logging;
using MFractor.Utilities;
using MFractor.Utilities.SyntaxWalkers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CodeSnippets
{
    /// <summary>
    /// A collection of extension methods for converting <see cref="ICodeSnippet"/>'s into synax.
    /// </summary>
    public static class CodeSnippetsExtensions
    {
        static readonly ILogger log = Logger.Create();

        /// <summary>
        /// Converts the <paramref name="snippet"/> into a <see cref="MemberDeclarationSyntax"/> list.
        /// </summary>
        /// <returns>The members list.</returns>
        /// <param name="snippet">Snippet.</param>
        public static IReadOnlyList<MemberDeclarationSyntax> AsMembersList(this ICodeSnippet snippet)
        {
            var code = snippet.ToString();

            var syntax = SyntaxFactory.ParseSyntaxTree(code);

            var walker = new MemberDeclarationSyntaxWalker();
            walker.Visit(syntax.GetRoot());

            if (!walker.Members.Any() && syntax != null)
            {
                log?.Warning("Failed to parse " + snippet + " and extract members");
            }

            return walker.Members;
        }

        /// <summary>
        /// Converts the <paramref name="snippet"/> into a <see cref="MemberDeclarationSyntax"/>.
        /// </summary>
        /// <returns>The members list.</returns>
        /// <param name="snippet">Snippet.</param>
        public static MemberDeclarationSyntax AsMember(this ICodeSnippet snippet)
        {
            var code = snippet.ToString();

            return  SyntaxFactory.ParseMemberDeclaration(code);
        }

        /// <summary>
        /// Converts the <paramref name="snippet"/> into a <see cref="StatementSyntax"/>.
        /// </summary>
        /// <returns>The statement.</returns>
        /// <param name="snippet">Snippet.</param>
        public static StatementSyntax AsStatement(this ICodeSnippet snippet)
        {
            var code = snippet.ToString();

            return SyntaxFactory.ParseStatement(code);
        }

        /// <summary>
        /// Converts the <paramref name="snippet"/> into a <see cref="CompilationUnitSyntax"/>.
        /// </summary>
        /// <returns>The compilation unit.</returns>
        /// <param name="snippet">Snippet.</param>
        public static CompilationUnitSyntax AsCompilationUnit(this ICodeSnippet snippet)
        {
            var code = snippet.ToString();

            var syntax = SyntaxFactory.ParseSyntaxTree(code);

            return syntax.GetCompilationUnitRoot();
        }

        /// <summary>
        /// Converts the <paramref name="snippet"/> into a <see cref="MFractor.Xml.XmlSyntaxTree"/>.
        /// </summary>
        /// <returns>The xml syntax.</returns>
        /// <param name="snippet">Snippet.</param>
        public static MFractor.Xml.XmlSyntaxTree AsXmlSyntax(this ICodeSnippet snippet)
        {
            var code = snippet.ToString();

            var parser = Resolver.Resolve<Xml.IXmlSyntaxParser>();

            var syntax = parser.ParseText(code);

            return syntax;
        }

        /// <summary>
        /// Converts the <paramref name="argumentName"/> into a string.
        /// </summary>
        /// <returns>The argument name.</returns>
        /// <param name="argumentName">Argument name.</param>
        public static string ToArgumentName(this ReservedCodeSnippetArgumentName argumentName)
        {
            return EnumHelper.GetEnumDescription(argumentName);
        }
    }
}
