using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using MFractor.Configuration;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.CodeSnippets
{
    /// <summary>
    /// The code snippet factory can be used to create new code snippets from a variety of sources.
    /// </summary>
    public interface ICodeSnippetFactory
    {
        /// <summary>
        /// A regular expression that can be used to isolate any code snippet arguments inside a piece of code. 
        /// </summary>
        Regex ArgumentsRegex { get; }

        /// <summary>
        /// Creates a new code snippet argument.
        /// </summary>
        /// <returns>The argument.</returns>
        /// <param name="name">The arguments name.</param>
        /// <param name="value">The default value of the argument.</param>
        /// <param name="description">A description of the code snippet argument.</param>
        /// <param name="order">The ordering (ascending) of the argument within the code snippet when displayed in UIs.</param>
        ICodeSnippetArgument CreateArgument(string name, string value = "", string description = "", int order = 0);

        /// <summary>
        /// Creates a new code snippet from the provided code.
        /// </summary>
        /// <returns>The snippet.</returns>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="code">Code.</param>
        ICodeSnippet CreateSnippet(string name, string description, string code);

        /// <summary>
        /// Creates a new code snippet.
        /// </summary>
        /// <returns>The snippet.</returns>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="code">Code.</param>
        /// <param name="arguments">Arguments.</param>
        ICodeSnippet CreateSnippet(string name, string description, string code, IReadOnlyList<ICodeSnippetArgument> arguments);

        /// <summary>
        /// Creates a new code snippet.
        /// </summary>
        /// <returns>The snippet.</returns>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="code">Code.</param>
        /// <param name="arguments">Arguments.</param>
        ICodeSnippet CreateSnippet(string name, string description, string code, IReadOnlyDictionary<string, string> arguments);

        /// <summary>
        /// Creates a new code snippet from an embedded resource.
        /// </summary>
        /// <returns>The snippet from embedded resource.</returns>
        /// <param name="assembly">Assembly.</param>
        /// <param name="snippetName">Snippet name.</param>
        ICodeSnippet CreateSnippetFromEmbeddedResource(Assembly assembly, string snippetName, IReadOnlyList<ICodeSnippetArgument> arguments = null);

        /// <summary>
        /// Creates the snippet from value.
        /// </summary>
        /// <returns>The snippet from value.</returns>
        /// <param name="snippetName">Snippet name.</param>
        /// <param name="code">Code.</param>
        ICodeSnippet CreateSnippetFromValue(string snippetName, string code);

        /// <summary>
        /// Extracts all arguments from the provided code.
        /// </summary>
        /// <returns>The arguments.</returns>
        /// <param name="code">Code.</param>
        IReadOnlyList<ICodeSnippetArgument> ExtractArguments(string code);

        /// <summary>
        /// Extracts the TextSpan of any code snippet arguments that are used in the provided code.
        /// </summary>
        /// <returns>The argument spans.</returns>
        /// <param name="code">Code.</param>
        IReadOnlyList<TextSpan> ExtractArgumentSpans(string code);

        ICodeSnippet GetDefaultSnippetForProperty(IConfigurable configurable, string name);

        ICodeSnippet GetDefaultSnippetForProperty(IConfigurable configurable, IConfigurableProperty property);
    }
}