// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pixeval.Utilities;

public enum SequenceComparison
{
    Sequential,
    Unordered
}

public static class Enumerates
{
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        var v = valueFactory();
        dictionary.Add(key, v);
        return v;
    }

    extension<T>(IEnumerable<T> source)
    {
        public IEnumerable<(int, T)> Indexed()
        {
            var counter = 0;
            foreach (var item in source)
            {
                yield return (counter++, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<T> AsList()
        {
            return source as IList<T> ?? [.. source];
        }
    }

    private class KeyedEqualityComparer<T, TKey>(Func<T, TKey> selector) : IEqualityComparer<T> where TKey : IEquatable<TKey>
    {
        public bool Equals(T? x, T? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return selector(x).Equals(selector(y));
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return selector(obj).GetHashCode();
        }
    }

    extension<T>(IEnumerable<T> @this)
    {
        public bool SequenceEquals<TKey>(IEnumerable<T> another,
            Func<T, TKey> keySelector,
            SequenceComparison comparison = SequenceComparison.Sequential)
            where TKey : IEquatable<TKey>
        {
            return comparison switch
            {
                SequenceComparison.Sequential => @this.SequenceEqual(another, new KeyedEqualityComparer<T, TKey>(keySelector)),
                SequenceComparison.Unordered => @this.Order().SequenceEqual(another.Order(), new KeyedEqualityComparer<T, TKey>(keySelector)), // not the fastest way, but still enough
                _ => throw new ArgumentOutOfRangeException(nameof(comparison))
            };
        }

        public bool SequenceEquals(IEnumerable<T> another,
            SequenceComparison comparison = SequenceComparison.Sequential,
            IEqualityComparer<T>? equalityComparer = null)
        {
            return comparison switch
            {
                SequenceComparison.Sequential => @this.SequenceEqual(another, equalityComparer),
                SequenceComparison.Unordered => @this.Order().SequenceEqual(another.Order(), equalityComparer), // not the fastest way, but still enough
                _ => throw new ArgumentOutOfRangeException(nameof(comparison))
            };
        }
    }

    extension<T>(IEnumerable<T?> enumerable)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> WhereNotNull()
        {
            return enumerable.Where(i => i is not null)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<T> WhereNotNull(Func<T, object?> keySelector)
        {
            return enumerable.Where(i => i is not null && keySelector(i) is not null)!;
        }
    }

    extension<T>(IEnumerable<T> src)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectNotNull<TResult>(Func<T, TResult?> selector) where TResult : notnull
        {
            return src.WhereNotNull().Select(selector).WhereNotNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TResult> SelectNotNull<TResult>(Func<T, object?> keySelector, Func<T, TResult> selector) where TResult : notnull
        {
            return src.WhereNotNull(keySelector).Select(selector).WhereNotNull();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool None()
        {
            return !src.Any();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool None(Func<T, bool> predicate)
        {
            return !src.Any(predicate);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ForEach(Action<T> action)
        {
            foreach (var t in src)
            {
                action(t);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrEmpty<T>(this IEnumerable<T>? enumerable)
    {
        return enumerable is not null && enumerable.Any();
    }

    public static async Task<ObservableCollection<T>> ToObservableCollectionAsync<T>(this IAsyncEnumerable<T> that)
    {
        var results = new ObservableCollection<T>();
        await foreach (var value in that)
        {
            results.Add(value);
        }

        return results;
    }

    public static void AddRange<T>(this ICollection<T> dest, IEnumerable<T> source)
    {
        foreach (var t in source)
            dest.Add(t);
    }

    /// <param name="dest">Collection to be updated</param>
    /// <typeparam name="T"></typeparam>
    extension<T>(IList<T> dest)
    {
        public void AddIfNotNull(T? source)
        {
            if (source is not null)
                dest.Add(source);
        }

        public int RemoveAll(Predicate<T> match)
        {
            var count = 0;

            for (var i = dest.Count - 1; i >= 0; i--)
            {
                if (!match(dest[i]))
                {
                    continue;
                }

                ++count;
                dest.RemoveAt(i);
            }

            return count;
        }

        /// <summary>
        /// Replace a collection by update transactions, best to use with ObservableCollection
        /// </summary>
        /// <param name="source"></param>
        public void ReplaceByUpdate(IEnumerable<T> source)
        {
            var enumerable = source as T[] ?? [.. source];
            if (enumerable.Length != 0)
            {
                _ = dest.RemoveAll(x => !enumerable.Contains(x));
                enumerable.Where(x => !dest.Contains(x)).ForEach(dest.Add);
            }
            else
            {
                dest.Clear();
            }
        }
    }
}
