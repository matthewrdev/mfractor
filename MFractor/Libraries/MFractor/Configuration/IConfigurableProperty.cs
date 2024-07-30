using System;
using System.Reflection;

namespace MFractor.Configuration
{
    /// <summary>
    /// A property on an IConfigurable that can be configured.
    /// </summary>
    public interface IConfigurableProperty
    {
        /// <summary>
        /// The owning object instance of this property
        /// </summary>
        /// <value>The owner instance.</value>
        object OwnerInstance { get; }

        /// <summary>
        /// A reflection PropertyInfo instance that can be used to mutate the property on the <see cref="OwnerInstance"/>.
        /// </summary>
        /// <value>The property.</value>
        PropertyInfo Property { get; }

        /// <summary>
        /// The name of this property.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// A description of this property.
        /// </summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>
        /// The properties original value.
        /// </summary>
        /// <value>The default value.</value>
        object DefaultValue { get; }

        /// <summary>
        /// The type that this property is.
        /// </summary>
        /// <value>The property type.</value>
        Type PropertyType { get; } 

        /// <summary>
        /// The properties current value.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; set; }
    }
}
