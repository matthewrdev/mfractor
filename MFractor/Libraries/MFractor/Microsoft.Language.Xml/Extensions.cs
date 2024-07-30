﻿using System;

namespace Microsoft.Language.Xml
{
    public static class Extensions
    {
        /// <summary>
        /// Search a sorted integer array for the target value in O(log N) time.
        /// </summary>
        /// <param name="array">The array of integers which must be sorted in ascending order.</param>
        /// <param name="value">The target value.</param>
        /// <returns>An index in the array pointing to the position where <paramref name="value"/> should be
        /// inserted in order to maintain the sorted order. All values to the right of this position will be
        /// strictly greater than <paramref name="value"/>. Note that this may return a position off the end
        /// of the array if all elements are less than or equal to <paramref name="value"/>.</returns>
        internal static int BinarySearchUpperBound(this int[] array, int value)
        {
            int low = 0;
            int high = array.Length - 1;

            while (low <= high)
            {
                int middle = low + ((high - low) >> 1);
                if (array[middle] > value)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return low;
        }

        internal static bool OverlapsWithAny(this TextSpan span, TextSpan[] otherSpans)
        {
            foreach (var other in otherSpans)
            {
                // TextSpan.OverlapsWith does not handle empty spans so
                // empty spans need to be handled explicitly.
                if (other.Length == 0)
                {
                    if (span.Contains(other.Start))
                        return false;
                }
                else
                {
                    if (span.OverlapsWith(other))
                        return true;
                }
            }
            return false;
        }

        internal static bool AnyContainsPosition(this TextSpan[] spans, int position)
        {
            foreach (var span in spans)
            {
                if (span.Contains(position) || (span.Length == 0 && span.Start == position))
                    return true;
                // Assume that spans are sorted
                if (span.Start > position)
                    return false;
            }
            return false;
        }
    }
}
