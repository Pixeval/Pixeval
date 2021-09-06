#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Enumerates.cs
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
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
                SequenceComparison.Unordered  => @this.OrderBy(Functions.Identity<T>()).SequenceEqual(another.OrderBy(Functions.Identity<T>()), equalityComparer), // not the fastest way, but still enough
                _                             => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null)
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
    }

    [PublicAPI]
    public static class EmptyEnumerators<T>
    {
        public static readonly IEnumerator<T> Sync = new List<T>().GetEnumerator();

        public static readonly IAsyncEnumerator<T> Async = new AdaptedAsyncEnumerator<T>(Sync);
    }

    [PublicAPI]
    public static class EmptyEnumerable<T>
    {
        public static readonly IEnumerable<T> Sync = new List<T>();

        public static readonly IAsyncEnumerable<T> Async = Sync.ToAsyncEnumerable();
    }
}