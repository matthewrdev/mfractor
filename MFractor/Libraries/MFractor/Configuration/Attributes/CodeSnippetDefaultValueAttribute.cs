using System;

namespace MFractor.Configuration.Attributes
{
    /// <summary>
    /// The code snippet value attribute can be used to specify a default code snippet value.
    /// <para/>
    /// This provides a simplier mechanism than using the <see cref="CodeSnippetResourceAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CodeSnippetDefaultValueAttribute : Attribute
    {
        /// <summary>
        /// The code snippet.
        /// </summary>
        /// <value>The code snippet</value>
        public string Code { get; }

        /// <summary>
        /// The name of this code snippet.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MFractor.Code.CodeSnippets.Attributes.CodeSnippetDefaultValueAttribute"/> class.
        /// </summary>
        /// <param name="code">The code value.</param>
        public CodeSnippetDefaultValueAttribute(string code, string description)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("message", nameof(code));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            Code = code;
            Description = description;
        }
    }
}
