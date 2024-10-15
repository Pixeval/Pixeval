#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/Enumerates.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

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

    public static IEnumerable<(int, T)> Indexed<T>(this IEnumerable<T> source)
    {
        var counter = 0;
        foreach (var item in source)
        {
            yield return (counter++, item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
    {
        return enumerable as IList<T> ?? enumerable.ToList();
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

    public static bool SequenceEquals<T, TKey>(this IEnumerable<T> @this,
        IEnumerable<T> another,
        Func<T, TKey> keySelector,
        SequenceComparison comparison = SequenceComparison.Sequential) 
    where TKey : IEquatable<TKey>
    {
        return comparison switch
        {
            SequenceComparison.Sequential => @this.SequenceEqual(another, new KeyedEqualityComparer<T,TKey>(keySelector)),
            SequenceComparison.Unordered => @this.Order().SequenceEqual(another.Order(), new KeyedEqualityComparer<T, TKey>(keySelector)), // not the fastest way, but still enough
            _ => ThrowUtils.ArgumentOutOfRange<SequenceComparison, bool>(comparison)
        };
    }

    public static bool SequenceEquals<T>(this IEnumerable<T> @this,
        IEnumerable<T> another,
        SequenceComparison comparison = SequenceComparison.Sequential,
        IEqualityComparer<T>? equalityComparer = null)
    {
        return comparison switch
        {
            SequenceComparison.Sequential => @this.SequenceEqual(another, equalityComparer),
            SequenceComparison.Unordered => @this.Order().SequenceEqual(another.Order(), equalityComparer), // not the fastest way, but still enough
            _ => ThrowUtils.ArgumentOutOfRange<SequenceComparison, bool>(comparison)
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
    public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> src, Func<T, TResult?> selector) where TResult : notnull
    {
        return src.WhereNotNull().Select(selector).WhereNotNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TResult> SelectNotNull<T, TResult>(this IEnumerable<T> src, Func<T, object?> keySelector, Func<T, TResult> selector) where TResult : notnull
    {
        return src.WhereNotNull(keySelector).Select(selector).WhereNotNull();
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

    /// <summary>
    /// https://stackoverflow.com/a/15407252/10439146 FirstOrDefault on value types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumerable"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FirstOrNull<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) where T : struct
    {
        var matches = enumerable.Where(predicate).Take(1).ToArray();
        return matches.Length is 0 ? null : matches[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? FirstOrNull<T>(this IEnumerable<T> enumerable) where T : struct
    {
        var matches = enumerable.Take(1).ToArray();
        return matches.Length is 0 ? null : matches[0];
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

    public static IEnumerable<T> Traverse<T>(this IEnumerable<T> src, Action<T> action)
    {
        var enumerable = src as T[] ?? src.ToArray();
        enumerable.ForEach(action);
        return enumerable;
    }

    /// <summary>
    /// Replace a collection by update transactions, best to use with ObservableCollection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="dest">Collection to be updated</param>
    /// <param name="source"></param>
    public static void ReplaceByUpdate<T>(this IList<T> dest, IEnumerable<T> source)
    {
        var enumerable = source as T[] ?? source.ToArray();
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

    public static void ReplaceByUpdate<T>(this ISet<T> dest, IEnumerable<T> source)
    {
        var enumerable = source as T[] ?? source.ToArray();
        if (enumerable.Length != 0)
        {
            dest.ToArray().Where(x => !enumerable.Contains(x)).ForEach(x => dest.Remove(x));
            dest.AddRange(enumerable);
        }
        else
        {
            dest.Clear();
        }
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

    public static Task<IEnumerable<TResult>> WhereAsync<TResult>(this Task<IEnumerable<TResult>> enumerable, Func<TResult, bool> selector)
    {
        return enumerable.ContinueWith(t => t.Result.Where(selector));
    }

    public static Task<IEnumerable<TResult>> SelectAsync<T, TResult>(this Task<IEnumerable<T>> enumerable, Func<T, TResult> selector)
    {
        return enumerable.ContinueWith(t => t.Result.Select(selector));
    }
}

public static class EmptyEnumerators<T>
{
    public static readonly IEnumerator<T> Sync = new List<T>().GetEnumerator();

    public static readonly IAsyncEnumerator<T> Async = new AdaptedAsyncEnumerator<T>(Sync);
}
