using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Utilities
{
    // Credit: https://stackoverflow.com/questions/1290603/how-to-get-the-index-of-an-element-in-an-ienumerable
    public static class EnumerableHelper
    {
        /// <summary>
        /// Creates an <see cref="IEnumerable{T}"/> containing the <paramref name="element"/>.
        /// </summary>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<T> AsEnumerable<T>(this T element)
        {
            yield return element;
        }

        /// <summary>
        /// Creates a <see cref="List{T}"/> containing the <paramref name="element"/>.
        /// </summary>
        /// <returns>The list.</returns>
        public static List<T> AsList<T>(this T element)
        {
            return element.AsEnumerable().ToList();
        }

        public static int IndexOf<T>(IEnumerable<T> enumerable, T value)
        {
            return enumerable
                .Select((a, i) => (a.Equals(value)) ? i : -1)
                .Max();
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T value
               , IEqualityComparer<T> comparer)
        {
            return enumerable
                .Select((a, i) => (comparer.Equals(a, value)) ? i : -1)
                .Max();
        }

        public static int FindIndex<T>(IEnumerable<T> enumerable, T instance)
        {
            return FindIndex(enumerable, item => item.Equals(instance));
        }

        public static int FindIndex<T>(IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            var item = enumerable.FirstOrDefault(predicate);

            if (item == null)
            {
                return -1;
            }

            return IndexOf(enumerable, item);
        }
    }
}
