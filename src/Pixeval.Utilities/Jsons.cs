#region Copyright (c) Pixeval/Pixeval.Utilities

// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2021 Pixeval.Utilities/Jsons.cs
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
using System.Runtime.CompilerServices;
using System.Text.Json;
using JetBrains.Annotations;

namespace Pixeval.Utilities
{
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