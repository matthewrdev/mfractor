using System.Collections.Generic;

namespace MFractor.CodeSnippets
{
    /// <summary>
    /// A code snippet is a piece of code where key sections of it are annotated with argument syntax ($placeholder$).
    /// <para/>
    /// A code snippet can originate from a file, an existing user defined snippet from the current IDE or any other extensible source.
    /// </summary>
    public interface ICodeSnippet
    {
        /// <summary>
        /// The name of this code snippet.
        /// <para/>
        /// If this snippet was generated from a file, this may be empty.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// A description of this code snippet.
        /// <para/>
        /// If this snippet was generated from a file, this may be empty.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// The arguments that are available for use in this code snippet.
        /// </summary>
        /// <value>The arguments.</value>
        IReadOnlyList<ICodeSnippetArgument> Arguments { get; }

        /// <summary>
        /// Gets the raw, templated code as it appears from its source.
        /// <para/>
        /// The templated code will still have it's argument syntax included (IE: pubilc $type$ $name$;).
        /// </summary>
        /// <value>The templated code.</value>
        string TemplatedCode { get; }

        /// <summary>
        /// Gets the formatted code.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        string GetFormattedCode(EmptyCodeSnippetArgumentMode mode);

        /// <summary>
        /// Gets a code snippet argument by name.
        /// </summary>
        /// <returns>The named argument or null if the argument does not exist.</returns>
        /// <param name="argumentName">Argument name.</param>
        ICodeSnippetArgument GetNamedArgument(string argumentName);

        /// <summary>
        /// Sets the value of a code snippet argument.
        /// <para/>
        /// If this argument does not already exist, this method will create a new ICodeSnippet argument and add it to the Arguments property.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        ICodeSnippet SetArgumentValue(string name, string value);

        /// <summary>
        /// Sets the value of a code snippet argument.
        /// <para/>
        /// If this argument does not already exist, this method will create a new ICodeSnippet argument and add it to the Arguments property.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        ICodeSnippet SetArgumentValue(ReservedCodeSnippetArgumentName name, string value);
    }
}
