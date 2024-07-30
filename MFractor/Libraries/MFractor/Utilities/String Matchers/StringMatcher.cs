using System;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Utilities.StringMatchers
{
    public abstract class StringMatcher
    {
        public abstract void SetFilter(string filter);
        public abstract bool CalcMatchRank(string name, out int matchRank);
        public abstract bool IsMatch(string name);
        public abstract int[] GetMatch(string text);

        public List<T> Match<T>(IEnumerable<T> items, Func<T, string> searchStringFunc)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (searchStringFunc == null)
            {
                throw new ArgumentNullException(nameof(searchStringFunc));
            }

            var matches = new HashSet<T>();

            foreach (var item in items)
            {
                var searchTerm = searchStringFunc(item);

                if (CalcMatchRank(searchTerm, out var rank))
                {
                    matches.Add(item);
                }
            }

            return matches.ToList();
        }

        public List<T> Match<T>(IEnumerable<T> items, Func<T, string[]> searchStringsFunc)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (searchStringsFunc == null)
            {
                throw new ArgumentNullException(nameof(searchStringsFunc));
            }


            var matches = new HashSet<T>();

            foreach (var item in items)
            {
                var searchTerms = searchStringsFunc(item);

                foreach (var searchTerm in searchTerms)
                {
                    if (CalcMatchRank(searchTerm, out var rank))
                    {
                        matches.Add(item);
                    }
                }
            }

            return matches.ToList();
        }

        public virtual StringMatcher Clone()
        {
            return (StringMatcher)MemberwiseClone();
        }
    }
}
