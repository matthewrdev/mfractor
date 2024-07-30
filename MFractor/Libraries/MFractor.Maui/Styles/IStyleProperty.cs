namespace MFractor.Maui.Styles
{
    /// <summary>
    /// A property value assignment for a style.
    /// </summary>
    public interface IStyleProperty
    {
        /// <summary>
        /// The primary key of the <see cref="MFractor.Maui.Data.Models.StyleDefinition"/> that this style property is owned by.
        /// </summary>
        public int ParentStyleKey { get; }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of this style property.
        /// </summary>
        public IStylePropertyValue Value { get; }

        /// <summary>
        /// The priority of this property setter.
        /// <para/>
        /// Lower values are higher priority.
        /// </summary>
        public int Priority { get; }
    }
}
