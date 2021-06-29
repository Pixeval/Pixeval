using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Util
{
    public class NumberOrStringToStringConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader switch
            {
                {TokenType: JsonTokenType.Number} => reader.GetUInt64().ToString(),
                {TokenType: JsonTokenType.String} => reader.GetString(),
                _                                 => throw new JsonException("Cannot unmarshal type string")
            };
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}