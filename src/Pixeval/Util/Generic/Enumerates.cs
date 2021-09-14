using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.WinUI.UI;
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

        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            var count = 0;

            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (!match(list[i]))
                {
                    continue;
                }
                ++count;
                list.RemoveAt(i);
            }

            return count;
        }

        public static void ResetView(this AdvancedCollectionView view)
        {
            var sourceCopy = view.Source.Cast<object>().ToList();
            foreach (var o in sourceCopy)
            {
                view.Remove(o);
            }
            view.AddRange(sourceCopy);
        }
    }
}