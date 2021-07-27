using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pixeval.Util
{
    public static class Enumerates
    {
        public static void AddSorted<T>(this IList<T> list, T item, Func<T?, T?, int> comparer)
        {
            var i = 0;
            while (i < list.Count && comparer(list[i], item) < 0)
            {
                i++;
            }

            list.Insert(i, item);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new(source);
        }

        public static void AddIfNotPresent<T>(this ObservableCollection<T> source, T value, IEqualityComparer<T> comparer)
        {
            if (!source.Contains(value, comparer))
            {
                source.Add(value);
            }
        }
    }
}