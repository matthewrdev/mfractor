namespace MFractor.CodeSnippets
{
    /// <summary>
    /// When transforming an <see cref="ICodeSnippet"/> into a string representation using <see cref="ICodeSnippet.GetFormattedCode(EmptyCodeSnippetArgumentMode)"/>,
    /// how should empty arguments in the final string value be represented?
    /// </summary>
    public enum EmptyCodeSnippetArgumentMode
    {
        /// <summary>
        /// For code snippet arguments that do not have a value set, replace their occurances with an empty string.
        /// </summary>
        Empty,

        /// <summary>
        /// For code snippet arguments that do not have a value set, replace their occurances with the name of the argument. EG: $base_type$.
        /// </summary>
        Name,

        /// <summary>
        /// For code snippet arguments that do not have a value set, replace their occurances with "notset".
        /// </summary>
        NotSetPlaceholderValue,
    }
}
