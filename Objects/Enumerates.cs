using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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

        public static void AddSorted<T>(this Collection<T> list, T item, IComparer<T> comparer)
        {
            if (comparer == null)
            {
                list.Add(item);
                return;
            }

            var i = 0;
            while (i < list.Count && comparer.Compare(list[i], item) < 0)
            {
                i++;
            }

            list.Insert(i, item);
        }

        public static IImmutableSet<R> ToImmutableSet<T, R>(this IEnumerable<T> enumerable, Func<T, R> function)
        {
            return enumerable == null ? new HashSet<R>().ToImmutableHashSet() : enumerable.Select(function).ToImmutableHashSet();
        }
    }
}