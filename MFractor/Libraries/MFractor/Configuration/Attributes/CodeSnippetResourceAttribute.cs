using System;

namespace MFractor.Configuration.Attributes
{
    /// <summary>
    /// The code snippet resource attribute can be used to direct the configuration engine to load a specific embedded resource
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false)]
    public class CodeSnippetResourceAttribute : Attribute
    {
        /// <summary>
        /// The path to the code snippet embedded resource, relative to the root of the project. For example: 'Resources/Snippets/custom_snippet.txt'
        /// </summary>
        /// <value>The path to the code snippet embedded resource, relative to the root of the project. For example: 'Resources/Snippets/custom_snippet.txt'</value>
        public string CodeSnippetFilePath { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MFractor.Code.CodeSnippets.Attributes.CodeSnippetResourceAttribute"/> class.
        /// </summary>
        /// <param name="codeSnippetFilePath">The path to the code snippet embedded resource, relative to the root of the assembly. For example: 'Resources/Snippets/custom_snippet.txt'</param>
        public CodeSnippetResourceAttribute(string codeSnippetFilePath)
        {
            CodeSnippetFilePath = codeSnippetFilePath;
        }
    }
}
