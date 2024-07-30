namespace MFractor.CodeSnippets
{
    /// <summary>
    /// A code snippet argument is 
    /// </summary>
    public interface ICodeSnippetArgument
    {
        /// <summary>
        /// The name of the code snippet argument.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// The name of the code snippet argument, formatted with a leading and trailing "$".
        /// <para/>
        /// For example, if this code snippets name is "base_type", it's formatted name is "$base_type$".
        /// </summary>
        string FormattedName { get; }

        /// <summary>
        /// The current value of this snippet argument.
        /// </summary>
        /// <value>The value.</value>
        string Value { get; set; }

        /// <summary>
        /// Get's the value of this code snippet argument, applying the provided <paramref name="mode"/> should this argument have no value.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        string GetValue(EmptyCodeSnippetArgumentMode mode = EmptyCodeSnippetArgumentMode.Empty);

        /// <summary>
        /// A description of code snippet argument.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// If the code snippet is displayed in a user interface, controls the ordering (ascending) that the argumetns should appear in.
        /// </summary>
        int Order { get; }
    }
}
