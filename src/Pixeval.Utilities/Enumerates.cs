using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Pixeval.Utilities
{
    [PublicAPI]
    public enum SequenceComparison
    {
        Sequential,
        Unordered
    }

    [PublicAPI]
    public static class Enumerates
    {
        public static bool SequenceEquals<T>(this IEnumerable<T> @this,
            IEnumerable<T> another,
            SequenceComparison comparison = SequenceComparison.Sequential,
            IEqualityComparer<T>? equalityComparer = null)
        {
            return comparison switch
            {
                SequenceComparison.Sequential => @this.SequenceEqual(another, equalityComparer),
                SequenceComparison.Unordered => @this.OrderBy(Functions.Identity<T>()).SequenceEqual(another.OrderBy(Functions.Identity<T>()), equalityComparer), // not the fastest way, but still enough
                _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        {
            return enumerable.Where(i => i is not null)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable, Func<T, object?> keySelector)
        {
            return enumerable.Where(i => i is not null && keySelector(i) is not null)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> src, Func<T, TResult> selector)
        {
            return src.WhereNotNull().Select(selector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> src, Func<T, object?> keySelector, Func<T, TResult> selector)
        {
            return src.WhereNotNull(keySelector).Select(selector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<T>(this IEnumerable<T> enumerable)
        {
            return !enumerable.Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
        {
            return !enumerable.Any(predicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var t in enumerable)
            {
                action(t);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
        {
            return enumerable is not null && enumerable.Any();
        }

        // https://stackoverflow.com/a/15407252/10439146 FirstOrDefault on value types
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrNull<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) where T : struct
        {
            var matches = enumerable.Where(predicate).Take(1).ToArray();
            return matches.Any() ? matches[0] : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FirstOrNull<T>(this IEnumerable<T> enumerable) where T : struct
        {
            var matches = enumerable.Take(1).ToArray();
            return matches.Any() ? matches[0] : null;
        }

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            return new AdaptedAsyncEnumerable<T>(source);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            return new ObservableCollection<T>(source);
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

        public static void AddIfAbsent<T>(this ICollection<T> collection, T item, IEqualityComparer<T>? comparer = null)
        {
            if (!collection.Contains(item, comparer))
            {
                collection.Add(item);
            } 
        }
    }

    [PublicAPI]
    public static class EmptyEnumerators<T>
    {
        public static readonly IEnumerator<T> Sync = new List<T>().GetEnumerator();

        public static readonly IAsyncEnumerator<T> Async = new AdaptedAsyncEnumerator<T>(Sync);
    }
}