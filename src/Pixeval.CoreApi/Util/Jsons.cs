#region Copyright (c) Pixeval/Mako

// MIT License
// 
// Copyright (c) Pixeval 2021 Mako/Jsons.cs
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
using System.Runtime.CompilerServices;
using System.Text.Json;
using JetBrains.Annotations;

namespace Pixeval.CoreApi.Util
{
    /// <summary>
    ///     Ext methods for JsonProperty and JsonElement.
    /// </summary>
    [PublicAPI]
    public static class Jsons
    {
        public static IEnumerable<JsonProperty> EnumerateObjectOrEmpty(this JsonElement? element)
        {
            return element?.EnumerateObject() as IEnumerable<JsonProperty> ?? Array.Empty<JsonProperty>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonElement GetProperty(this JsonProperty jsonElement, string prop)
        {
            return jsonElement.Value.GetProperty(prop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string prop)
        {
            return element.TryGetProperty(prop, out var result) ? result : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JsonElement? GetPropertyOrNull(this JsonProperty property, string prop)
        {
            return property.Value.TryGetProperty(prop, out var result) ? result : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetPropertyString(this JsonElement jsonElement)
        {
            return jsonElement.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetPropertyString(this JsonElement jsonElement, string prop)
        {
            return jsonElement.GetProperty(prop).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetPropertyString(this JsonProperty jsonProperty)
        {
            return jsonProperty.Value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? GetPropertyString(this JsonProperty jsonProperty, string prop)
        {
            return jsonProperty.Value.GetProperty(prop).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetPropertyLong(this JsonProperty jsonProperty, string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetInt64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetPropertyLong(this JsonElement jsonElement, string prop)
        {
            return jsonElement.GetProperty(prop).GetInt64();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset GetPropertyDateTimeOffset(this JsonProperty jsonProperty)
        {
            return jsonProperty.Value.GetDateTimeOffset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset GetPropertyDateTimeOffset(this JsonProperty jsonProperty, string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetDateTimeOffset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime GetPropertyDateTime(this JsonProperty jsonProperty)
        {
            return jsonProperty.Value.GetDateTime();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime GetPropertyDateTime(this JsonProperty jsonProperty, string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetDateTime();
        }
    }
}