using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class CoreExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> e, T item)
        {
            var found = false;
            var index = e.TakeWhile(i => {
                found = EqualityComparer<T>.Default.Equals(i, item);
                return !found;
            }).Count();

            return found ? index : -1;
        }
    }
}
