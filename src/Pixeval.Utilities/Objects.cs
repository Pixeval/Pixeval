#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2023 Pixeval.Utilities/Objects.cs
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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;

namespace Pixeval.Utilities;

public static class Objects
{
    public static readonly IEqualityComparer<string> CaseIgnoredComparer = new CaseIgnoredStringComparer();

    /// <summary>
    /// Normalize a double to a value between 0 and 1
    /// </summary>
    /// <param name="value">The value to be normalized</param>
    /// <param name="max"></param>
    /// <param name="min"></param>
    /// <returns>[0, 1]</returns>
    public static double Normalize(this double value, int max, int min)
    {
        return (value - min) / (max - min);
    }

    public static TReturn Using<T, TReturn>(this T disposable, Func<T, TReturn> action) where T : IDisposable
    {
        return action(disposable);
    }

    /// <summary>
    /// 当<paramref name="str"/>为<see langword="const"/>时使用<see cref="GeneratedRegexAttribute"/>代替
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Regex ToRegex(this string str)
    {
        return new Regex(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrEmpty(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNotNullOrBlank(this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrBlank(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static byte[] GetBytes(this string str, Encoding? encoding = null)
    {
        return encoding?.Let(e => e.GetBytes(str)) ?? Encoding.UTF8.GetBytes(str);
    }

    public static string GetString(this byte[] bytes, Encoding? encoding = null)
    {
        return encoding?.Let(e => e.GetString(bytes)) ?? Encoding.UTF8.GetString(bytes);
    }

    public static string GetString(this MemoryOwner<byte> bytes, Encoding? encoding = null)
    {
        using (bytes)
        {
            return encoding?.Let(e => e.GetString(bytes.Span)) ?? Encoding.UTF8.GetString(bytes.Span);
        }
    }

    public static async Task<string> HashAsync<THash>(this string str) where THash : HashAlgorithm, new()
    {
        using var hasher = new THash();
        await using var memoryStream = new MemoryStream(str.GetBytes());
        var bytes = await hasher.ComputeHashAsync(memoryStream).ConfigureAwait(false);
        return bytes.Select(b => b.ToString("x2")).Aggregate(string.Concat);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCase(this string str1, string str2)
    {
        return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns <see cref="Result{T}.Failure" /> if the status code does not indicating success
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="url"></param>
    /// <param name="exceptionSelector"></param>
    /// <returns></returns>
    public static async Task<Result<string>> GetStringResultAsync(this HttpClient httpClient, string url, Func<HttpResponseMessage, Task<Exception>>? exceptionSelector = null)
    {
        var responseMessage = await httpClient.GetAsync(url).ConfigureAwait(false);
        return responseMessage.IsSuccessStatusCode ? Result<string>.AsSuccess(await responseMessage.Content.ReadAsStringAsync()) : Result<string>.AsFailure(exceptionSelector is { } selector ? await selector.Invoke(responseMessage).ConfigureAwait(false) : null);
    }

    public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
        return Task.WhenAll(tasks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNull(this object? obj)
    {
        return obj is null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Negate(this bool b)
    {
        return !b;
    }

    public static string RemoveSurrounding(this string str, string prefix, string suffix)
    {
        return str[(str.StartsWith(prefix) ? prefix.Length : 0)..(str.EndsWith(suffix) ? ^suffix.Length : str.Length)];
    }

    public static async Task<string> HashAsync<T>(this T algorithm, byte[] bytes) where T : HashAlgorithm
    {
        await using var memoryStream = new MemoryStream(bytes);
        return (await algorithm.ComputeHashAsync(memoryStream)).ToHexString();
    }

    public static string ToHexString(this byte[] bytes)
    {
        return bytes.Select(b => b.ToString("X2")).Aggregate((s1, s2) => s1 + s2);
    }

    public static string Format(this string str, params object?[] args)
    {
        return string.Format(str, args);
    }

    /// <summary>
    /// Start inclusive, end inclusive
    /// </summary>
    public static bool InRange(this double i, (double, double) range)
    {
        var (startInclusive, endInclusive) = range;
        return i >= startInclusive && i <= endInclusive;
    }

    public static double CoerceIn(this double i, (double, double) range)
    {
        var (startInclusive, endInclusive) = range;
        return Math.Max(startInclusive, Math.Min(i, endInclusive));
    }

    public static int CoerceIn(this int i, (int, int) range)
    {
        var (startInclusive, endInclusive) = range;
        return Math.Max(startInclusive, Math.Min(i, endInclusive));
    }

    public static IEnumerable<TEnum> GetEnumValues<TEnum>(this Type type)
    {
        return type.GetEnumValues().Cast<TEnum>();
    }

    public static bool Inverse(ref this bool b)
    {
        return b = !b;
    }

    public static async Task<R?> UnwrapOrElseAsync<R>(this Task<Result<R>> task, R? orElse)
    {
        return (await task).UnwrapOrElse(orElse);
    }

    public static bool IsValidRegexPattern(this string pattern)
    {
        if (pattern.IsNullOrEmpty())
        {
            return false;
        }

        try
        {
            _ = new Regex(pattern, RegexOptions.None, new TimeSpan(0, 0, 2)).IsMatch(string.Empty);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private class CaseIgnoredStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y)
        {
            return x is not null && y is not null && x.EqualsIgnoreCase(y);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
