// Copyright (c) Pixeval.Utilities.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Pixeval.Utilities;

public static class Jsons
{
    public static IEnumerable<JsonProperty> EnumerateObjectOrEmpty(this JsonElement? element)
    {
        return element?.EnumerateObject() as IEnumerable<JsonProperty> ?? [];
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
