using System;

namespace MFractor.Attributes
{
    /// <summary>
    /// The import name attribute can be used to name enum values for importing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ImportNameAttribute : Attribute
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Attributes.ImportNameAttribute"/> class.
        /// </summary>
        /// <param name="name">The name to be used when exporting the value.</param>
        public ImportNameAttribute(string name) => Name = name;
    }
}
