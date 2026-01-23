// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pixeval.Models.Extensions;

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(DateTimeOffset))]
public partial class BasicTypeSerializerContext : JsonSerializerContext;

public static class DictionaryExtensions
{
    extension(Dictionary<string, JsonElement> dictionary)
    {
        public bool TryGetTarget<T>(string key, out T? result)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                try
                {
                    if (value.Deserialize(typeof(T), BasicTypeSerializerContext.Default) is T v)
                    {
                        result = v;
                        return true;
                    }
                }
                catch
                {
                    // ignored
                }
            }
            result = default;
            return false;
        }

        public T TryGetTargetOrAddDefault<T>(string key, T defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                try
                {
                    if (value.Deserialize(typeof(T), BasicTypeSerializerContext.Default) is T v)
                        return v;
                }
                catch
                {
                    // ignored
                }
            }
            dictionary.SetTarget(key, defaultValue);
            return defaultValue;
        }

        public void SetTarget<T>(string key, T value)
        {
            dictionary[key] = JsonSerializer.SerializeToElement(value, typeof(T), BasicTypeSerializerContext.Default);
        }
    }
}
