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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(i => i);
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

    // https://stackoverflow.com/a/15407252/10439146 FirstOrDefault on value types
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

    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        return new AdaptedAsyncEnumerable<T>(source);
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }

    public static T[] ArrayOf<T>(params T[] t)
    {
        return t;
    }

    public static IEnumerable<T> EnumerableOf<T>(params T[] t)
    {
        return ArrayOf(t);
    }

    public static IEnumerable<T> Traverse<T>(this IEnumerable<T> src, Action<T> action)
    {
        var enumerable = src as T[] ?? src.ToArray();
        enumerable.ForEach(action);
        return enumerable;
    }

    /// <summary>
    ///     Replace a collection by update transactions, best to use with ObservableCollection
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
