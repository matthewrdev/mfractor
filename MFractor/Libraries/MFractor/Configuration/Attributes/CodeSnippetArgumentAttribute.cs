using System;
using MFractor.CodeSnippets;
using MFractor.Utilities;

namespace MFractor.Configuration.Attributes
{
    /// <summary>
    /// Specifies a user configurable code snippet argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =true)]
    public class CodeSnippetArgumentAttribute : Attribute
    {
        public string Name { get; }

        public string Description { get; }

        public int Order { get; } = 0;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MFractor.Code.CodeSnippets.Attributes.CodeSnippetArgumentAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the code snippet argument, excluding the '$'.</param>
        /// <param name="description">Description.</param>
        public CodeSnippetArgumentAttribute(string name,
                                            string description,
                                            int order = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty", nameof(name));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException($"'{nameof(description)}' cannot be null or empty", nameof(description));
            }

            if (name.StartsWith("$"))
            {
                name = name.Remove(0, 1);
            }

            if (name.EndsWith("$"))
            {
                name = name.Substring(0, name.Length - 1);
            }

            Name = name;
            Description = description;
            Order = order;
        }

        public CodeSnippetArgumentAttribute(ReservedCodeSnippetArgumentName name,
                                            string description,
                                            int order = 0)
            : this(EnumHelper.GetEnumDescription(name), description, order)
        {
        }
    }
}
