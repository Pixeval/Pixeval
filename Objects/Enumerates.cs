using System.Collections.Generic;
using System.Linq;

namespace Pixeval.Objects
{
    public static class Enumerates
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        public static void AddRange<T>(this ICollection<T> dst, IEnumerable<T> src)
        {
            foreach (var t in src)
            {
                dst.Add(t);
            }
        }
    }
}