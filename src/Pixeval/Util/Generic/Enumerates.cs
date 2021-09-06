using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pixeval.CoreApi.Util;

namespace Pixeval.Util.Generic
{
    public static class Enumerates
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new(source);
        }

        public static T[] ArrayOf<T>(params T[] t) => t;

        public static IEnumerable<T> EnumerableOf<T>(params T[] t) => ArrayOf(t);

        public static IEnumerable<T> Traverse<T>(this IEnumerable<T> src, Action<T> action)
        {
            var enumerable = src as T[] ?? src.ToArray();
            enumerable.ForEach(action);
            return enumerable;
        }

        public static void AddRange<T>(this ICollection<T> dest, IEnumerable<T> source)
        {
            foreach (var t in source)
            {
                dest.Add(t);
            }
        }
    }
}