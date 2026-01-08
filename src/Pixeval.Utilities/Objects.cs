// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;

namespace Pixeval.Utilities;

public static class Objects
{
    public static readonly IEqualityComparer<string> CaseIgnoredComparer = new CaseIgnoredStringComparer();

    public static TReturn Using<T, TReturn>(this T disposable, Func<T, TReturn> action) where T : IDisposable
    {
        return action(disposable);
    }

    extension(string? str)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotNullOrEmpty()
        {
            return !string.IsNullOrEmpty(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotNullOrBlank()
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrBlank()
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str)
    {
        return string.IsNullOrEmpty(str);
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

    /// <param name="str"></param>
    public static string Format(this string str, params object?[] args)
    {
        return string.Format(str, args);
    }

    /// <param name="str"></param>
    extension(string str)
    {
        public async Task<string> HashAsync<THash>() where THash : HashAlgorithm, new()
        {
            using var hasher = new THash();
            await using var memoryStream = new MemoryStream(str.GetBytes());
            var bytes = await hasher.ComputeHashAsync(memoryStream).ConfigureAwait(false);
            return bytes.Select(b => b.ToString("x2")).Aggregate(string.Concat);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EqualsIgnoreCase(string str2)
        {
            return string.Equals(str, str2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 当<paramref name="str"/>为<see langword="const"/>时使用<see cref="GeneratedRegexAttribute"/>代替
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Regex ToRegex()
        {
            return new Regex(str);
        }

        public byte[] GetBytes(Encoding? encoding = null)
        {
            return encoding?.Let(e => e.GetBytes(str)) ?? Encoding.UTF8.GetBytes(str);
        }

        public string RemoveSurrounding(string prefix, string suffix)
        {
            return str[(str.StartsWith(prefix) ? prefix.Length : 0)..(str.EndsWith(suffix) ? ^suffix.Length : str.Length)];
        }

        public bool IsValidRegexPattern()
        {
            if (str.IsNullOrEmpty())
            {
                return false;
            }

            try
            {
                _ = new Regex(str, RegexOptions.None, new TimeSpan(0, 0, 2)).IsMatch(string.Empty);
            }
            catch
            {
                return false;
            }

            return true;
        }
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

    public static async Task<string> HashAsync<T>(this T algorithm, byte[] bytes) where T : HashAlgorithm
    {
        await using var memoryStream = new MemoryStream(bytes);
        return (await algorithm.ComputeHashAsync(memoryStream)).ToHexString();
    }

    public static string ToHexString(this byte[] bytes)
    {
        return bytes.Select(b => b.ToString("X2")).Aggregate((s1, s2) => s1 + s2);
    }

    /// <param name="i">The value to be normalized</param>
    extension(double i)
    {
        /// <summary>
        /// Start inclusive, end inclusive
        /// </summary>
        public bool InRange((double, double) range)
        {
            var (startInclusive, endInclusive) = range;
            return i >= startInclusive && i <= endInclusive;
        }

        public double CoerceIn((double, double) range)
        {
            var (startInclusive, endInclusive) = range;
            return Math.Max(startInclusive, Math.Min(i, endInclusive));
        }

        /// <summary>
        /// Normalize a double to a value between 0 and 1
        /// </summary>
        /// <param name="max"></param>
        /// <param name="min"></param>
        /// <returns>[0, 1]</returns>
        public double Normalize(int max, int min)
        {
            return (i - min) / (max - min);
        }
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

    extension(CancellationTokenSource cancellationTokenSource)
    {
        public void TryCancel()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
        }

        public void TryCancelDispose()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public async Task TryCancelAsync()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                await cancellationTokenSource.CancelAsync();
        }

        public async Task TryCancelDisposeAsync()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
                await cancellationTokenSource.CancelAsync();
            cancellationTokenSource.Dispose();
        }
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
