#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Objects.cs
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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Pixeval.CoreApi.Util
{
    [PublicAPI]
    public static class Objects
    {
        public static readonly IEqualityComparer<string> CaseIgnoredComparer = new CaseIgnoredStringComparer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Regex ToRegex(this string str)
        {
            return new(str);
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
        public static bool IsNullOrEmpty(this string? str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static byte[] GetBytes(this string str, Encoding? encoding = null)
        {
            return encoding?.Let(e => e!.GetBytes(str)) ?? Encoding.UTF8.GetBytes(str);
        }

        public static string GetString(this byte[] bytes, Encoding? encoding = null)
        {
            return encoding?.Let(e => e!.GetString(bytes)) ?? Encoding.UTF8.GetString(bytes);
        }

        public static string GetString(this MemoryOwner<byte> bytes, Encoding? encoding = null)
        {
            using (bytes)
            {
                return encoding?.Let(e => e!.GetString(bytes.Span)) ?? Encoding.UTF8.GetString(bytes.Span);
            }
        }

        public static async Task<string> HashAsync<THash>(this string str) where THash : HashAlgorithm, new()
        {
            using var hasher = new THash();
            await using var memoryStream = new MemoryStream(str.GetBytes());
            var bytes = await hasher.ComputeHashAsync(memoryStream).ConfigureAwait(false);
            return bytes.Select(b => b.ToString("x2")).Aggregate(string.Concat);
        }

        public static Task<HttpResponseMessage> GetResponseHeader(this HttpClient client, string url)
        {
            return client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        }

        public static async Task<string?> ToJsonAsync<TEntity>(this TEntity? obj, Action<JsonSerializerOptions>? serializerOptionConfigure = null)
        {
            if (obj is null)
            {
                return null;
            }

            await using var memoryStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(memoryStream, obj, new JsonSerializerOptions().Apply(option => serializerOptionConfigure?.Invoke(option))).ConfigureAwait(false);
            return memoryStream.ToArray().GetString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContractAnnotation("obj:notnull => notnull; obj:null => null")]
        public static string? ToJson(this object? obj, Action<JsonSerializerOptions>? serializerOptionConfigure = null)
        {
            return obj?.Let(o => JsonSerializer.Serialize(o, new JsonSerializerOptions().Apply(option => serializerOptionConfigure?.Invoke(option))));
        }

        public static async ValueTask<TEntity?> FromJsonAsync<TEntity>(this IMemoryOwner<byte> bytes, Action<JsonSerializerOptions>? serializerOptionConfigure = null)
        {
            using (bytes)
            {
                await using var stream = bytes.Memory.AsStream();
                return await JsonSerializer.DeserializeAsync<TEntity>(stream, new JsonSerializerOptions().Apply(option => serializerOptionConfigure?.Invoke(option))).ConfigureAwait(false);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEntity? FromJson<TEntity>(this string str, Action<JsonSerializerOptions>? serializerOptionConfigure = null)
        {
            return JsonSerializer.Deserialize<TEntity>(str, new JsonSerializerOptions().Apply(option => serializerOptionConfigure?.Invoke(option)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Returns <see cref="Result{T}.Failure" /> if the status code does not indicating success
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <param name="exceptionSelector"></param>
        /// <returns></returns>
        public static async Task<Result<string>> GetStringResultAsync(this HttpClient httpClient, string url, Func<HttpResponseMessage, Task<Exception>>? exceptionSelector = null)
        {
            var responseMessage = await httpClient.GetAsync(url).ConfigureAwait(false);
            return !responseMessage.IsSuccessStatusCode ? Result<string>.OfFailure(exceptionSelector is { } selector ? await selector.Invoke(responseMessage).ConfigureAwait(false) : null) : Result<string>.OfSuccess(await responseMessage.Content.ReadAsStringAsync());
        }

        public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>
        ///     Copy all the nonnull properties of <paramref name="this" /> to the same properties of <paramref name="another" />
        /// </summary>
        /// <param name="this"></param>
        /// <param name="another"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T With<T>(this T @this, T another)
        {
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ForEach(info => info.GetValue(@this)?.Let(o => info.SetValue(another, o)));
            return another;
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
}