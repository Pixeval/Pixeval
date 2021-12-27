#region Copyright (c) Pixeval/Pixeval.Utilities
// GPL v3 License
// 
// Pixeval/Pixeval.Utilities
// Copyright (c) 2021 Pixeval.Utilities/NumberOrStringToStringConverter.cs
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
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pixeval.Utilities;

public class NumberOrStringToStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader switch
        {
            { TokenType: JsonTokenType.Number } => reader.GetUInt64().ToString(),
            { TokenType: JsonTokenType.String } => reader.GetString(),
            _ => throw new JsonException("Cannot unmarshal type string")
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}