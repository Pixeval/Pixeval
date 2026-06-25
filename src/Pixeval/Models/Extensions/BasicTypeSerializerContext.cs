// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Pixeval.Models.Extensions;

public static class DictionaryExtensions
{
    extension(Dictionary<string, object?> dictionary)
    {
        public bool TryGetTarget<T>(string key, [NotNullWhen(true)] out T? result)
        {
            if (dictionary.TryGetValue(key, out var value) && value is T v)
            {
                result = v;
                return true;
            }

            result = default;
            return false;
        }

        public T TryGetTargetOrAddDefault<T>(string key, T defaultValue)
        {
            if (dictionary.TryGetValue(key, out var value) && value is T v)
                return v;
            
            dictionary[key] = defaultValue;
            return defaultValue;
        }
    }
}
