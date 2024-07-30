using System;

namespace MFractor.Documentation
{
    /// <summary>
    /// Marks that this class should be included in MFractor's auto-generated documentation.
    /// <para/>
    /// Classes that implement <see cref="IAmDocumented"/> should have a default, parameterless constructor that MFractor can use create a new instance of the element so it can read it's documentation.
    /// </summary>
    public interface IAmDocumented
    {
        /// <summary>
        /// A short (3-6) word description of this element.
        /// <para/>
        /// This is the title of the documentation section of this element and may also be used in analytics.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// A complete overview of the element and what it does; something like a "mini-tutorial".
        /// <para/>
        /// This documentation will be exposed in the final auto-generated documentation.
        /// </summary>
        /// <value>The documentation.</value>
        string Documentation { get; }
    }
}
