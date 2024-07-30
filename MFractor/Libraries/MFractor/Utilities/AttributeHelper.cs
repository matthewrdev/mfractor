using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Utilities
{
    /// <summary>
    /// A helper class for working with attributes on <see cref="Type"/>.
    /// </summary>
    public static class AttributeHelper
    {
        /// <summary>
        /// Does the provided <paramref name="type"/> have a <typeparamref name="TAttribute"/> attribute?
        /// </summary>
        /// <returns><c>true</c>, if attribute was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="inherit">If set to <c>true</c> inherit.</param>
        /// <typeparam name="TAttribute">The 1st type parameter.</typeparam>
        public static bool HasAttribute<TAttribute>(Type type, bool inherit = true) where TAttribute : Attribute
        {
            var matches = type.GetCustomAttributes(typeof(TAttribute), inherit);

            return matches.Any();
        }

        /// <summary>
        /// Gets the first <typeparamref name="TAttribute"/> attribute on <paramref name="type"/>.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="type">Type.</param>
        /// <param name="inherit">If set to <c>true</c> inherit.</param>
        /// <typeparam name="TAttribute">The 1st type parameter.</typeparam>
        public static TAttribute GetAttribute<TAttribute>(Type type, bool inherit = true) where TAttribute : Attribute
        {
            var matches = type.GetCustomAttributes(typeof(TAttribute), inherit);

            return matches.Cast<TAttribute>().FirstOrDefault();
        }
        /// <summary>
        /// Gets the <typeparamref name="TAttribute"/> attributes on <paramref name="type"/>.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="type">Type.</param>
        /// <param name="inherit">If set to <c>true</c> inherit.</param>
        /// <typeparam name="TAttribute">The 1st type parameter.</typeparam>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(Type type, bool inherit = true) where TAttribute : Attribute
        {
            var matches = type.GetCustomAttributes(typeof(TAttribute), inherit);

            return matches.Cast<TAttribute>().ToList();
        }
    }
}
