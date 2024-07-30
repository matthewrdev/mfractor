using System;

namespace MFractor.Configuration.Attributes
{
    /// <summary>
    /// Exposes a property for configuration by a user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExportPropertyAttribute : Attribute
    {
        /// <summary>
        /// A description of this property suitable for displaying within user interfaces or included into documentation.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Configuration.Attributes.ExportPropertyAttribute"/> class.
        /// </summary>
        /// <param name="description"> description of this property suitable for displaying within user interfaces or included into documentation.</param>
        public ExportPropertyAttribute(string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
        }
    }
}
