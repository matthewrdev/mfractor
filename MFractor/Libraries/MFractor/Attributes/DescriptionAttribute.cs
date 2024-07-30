using System;

namespace MFractor.Attributes
{
    /// <summary>
    /// The description attribute can be used to describe enum values for display.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DescriptionAttribute : Attribute
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Attributes.DescriptionAttribute"/> class.
        /// </summary>
        /// <param name="description">Description.</param>
        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
