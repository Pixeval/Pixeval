// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Pixeval.Utilities;

public static class Jsons
{
    extension(JsonElement? element)
    {
        public IEnumerable<JsonProperty> EnumerateObjectOrEmpty()
        {
            return element?.EnumerateObject() as IEnumerable<JsonProperty> ?? [];
        }
    }

    extension(JsonProperty jsonElement)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonElement GetProperty(string prop)
        {
            return jsonElement.Value.GetProperty(prop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonElement? GetPropertyOrNull(string prop)
        {
            return jsonElement.Value.TryGetProperty(prop, out var result) ? result : null;
        }
    }

    extension(JsonElement jsonElement)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetPropertyString()
        {
            return jsonElement.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetPropertyString(string prop)
        {
            return jsonElement.GetProperty(prop).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JsonElement? GetPropertyOrNull(string prop)
        {
            return jsonElement.TryGetProperty(prop, out var result) ? result : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetPropertyLong(string prop)
        {
            return jsonElement.GetProperty(prop).GetInt64();
        }
    }

    extension(JsonProperty jsonProperty)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetPropertyString()
        {
            return jsonProperty.Value.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetPropertyString(string prop)
        {
            return jsonProperty.Value.GetProperty(prop).ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetPropertyLong(string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetInt64();
        }
    }

    extension(JsonProperty jsonProperty)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset GetPropertyDateTimeOffset()
        {
            return jsonProperty.Value.GetDateTimeOffset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTimeOffset GetPropertyDateTimeOffset(string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetDateTimeOffset();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetPropertyDateTime()
        {
            return jsonProperty.Value.GetDateTime();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetPropertyDateTime(string prop)
        {
            return jsonProperty.Value.GetProperty(prop).GetDateTime();
        }
    }
}
