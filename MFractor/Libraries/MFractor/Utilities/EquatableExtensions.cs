using System;
using System.Linq;

namespace System
{
    /// <summary>
    /// Provides useful basic extensions to types that implements the <see cref="Equatable<T>"/> interface.
    /// </summary>
    public static class EquatableExtensions
    {
        /// <summary>
        /// Check if the current value is equals to at least one of the provided options.
        /// </summary>
        /// <param name="options">The options to check against the current instance.</param>
        /// <returns><c>true</c> if at least one of the values of the list matches.</returns>
        public static bool In<T>(this T value, params T[] options) where T : IEquatable<T> => options.Any(o => o.Equals(value));

        /// <summary>
        /// Check if the current value is not equals to all of the provided options.
        /// </summary>
        /// <param name="options">The options to check against the current instance.</param>
        /// <returns><c>true</c> if none of the options matches.</returns>
        public static bool NotIn<T>(this T value, params T[] options) where T : IEquatable<T> => !value.In(options);
    }
}
