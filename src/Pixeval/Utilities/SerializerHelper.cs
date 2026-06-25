// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using SharpYaml;

namespace Pixeval.Utilities;

public static class SerializerHelper
{
    extension(JsonSerializer)
    {
        public static void SerializeToFile<TValue>(string filePath, TValue value, JsonTypeInfo<TValue> info)
        {
            using var stream = File.OpenWriteOrTruncate(filePath);
            JsonSerializer.Serialize(stream, value, info);
        }

        public static async Task SerializeToFileAsync<TValue>(string filePath, TValue value, JsonTypeInfo<TValue> info)
        {
            await using var stream = File.OpenAsyncWriteOrTruncate(filePath);
            await JsonSerializer.SerializeAsync(stream, value, info);
        }

        public static TValue? DeserializeFile<TValue>(string filePath, JsonTypeInfo<TValue> info)
        {
            using var stream = File.OpenAsyncRead(filePath);
            return JsonSerializer.Deserialize(stream, info);
        }

        public static async Task<TValue?> DeserializeFileAsync<TValue>(string filePath, JsonTypeInfo<TValue> info)
        {
            await using var stream = File.OpenAsyncRead(filePath);
            return await JsonSerializer.DeserializeAsync(stream, info);
        }
    }

    extension(YamlSerializer)
    {
        public static void SerializeToFile<TValue>(string filePath, TValue value, YamlTypeInfo<TValue> context)
        {
            using var stream = File.OpenWriteOrTruncate(filePath);
            YamlSerializer.Serialize(stream, value, context);
        }

        public static TValue? DeserializeFile<TValue>(string filePath, YamlTypeInfo<TValue> info)
        {
            using var stream = File.OpenAsyncRead(filePath);
            return YamlSerializer.Deserialize(stream, info);
        }

        public static void Serialize<TValue>(Stream stream, TValue value, YamlTypeInfo<TValue> context)
        {
            YamlSerializer.Serialize(stream, value, context);
        }

        public static TValue? Deserialize<TValue>(Stream stream, YamlTypeInfo<TValue> info)
        {
            return YamlSerializer.Deserialize(stream, info);
        }
    }
}
