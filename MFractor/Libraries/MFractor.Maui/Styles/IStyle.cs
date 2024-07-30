using System.Collections.Generic;

namespace MFractor.Maui.Styles
{
    /// <summary>
    /// A style that can be applied to a VisualElement.
    /// </summary>
    public interface IStyle
    {
        /// <summary>
        /// The fully qualified meta-type that this style targets.
        /// </summary>
        public string TargetType { get; }

        /// <summary>
        /// The name of this style.
        /// <para/>
        /// Can be empty if this is an implicit style (see <see cref="IsImplicit"/>).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is this style implicit?
        /// <para/>
        /// If a style does not specify a <see cref="Name"/>, then the style is automatically applied to the <see cref="TargetType"/>.
        /// <para/>
        /// Implicit styles are automatically applied to the elements where the <see cref="TargetType"/> matches.
        /// </summary>
        public bool IsImplicit { get; }

        /// <summary>
        /// An ordered list of the static resources that this style is composed of.
        /// </summary>
        public IReadOnlyList<string> InheritanceChain { get; }

        /// <summary>
        /// The properties in this style.
        /// <para/>
        /// This includes all properties included in the defined <see cref="InheritanceChain"/>.
        /// </summary>
        public IStylePropertyCollection Properties { get; }

    }
}
