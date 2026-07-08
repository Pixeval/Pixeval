using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pixeval.Models.McpServer;

internal sealed record JsonSchemaObject
{
    public JsonElement Type { get; init; }

    public IReadOnlyDictionary<string, JsonSchemaObject>? Properties { get; init; }

    public IReadOnlyList<string>? Required { get; init; }

    public IReadOnlyList<JsonSchemaObject>? AnyOf { get; init; }

    public IReadOnlyList<JsonSchemaObject>? OneOf { get; init; }

    public IReadOnlyList<JsonSchemaObject>? AllOf { get; init; }

    public IReadOnlyList<JsonElement>? Enum { get; init; }

    public JsonElement? Default { get; init; }

    public JsonSchemaObject? Items { get; init; }

    [JsonPropertyName("$ref")] public string? Reference { get; init; }

    public string? Description { get; init; }

    public IReadOnlyList<McpToolParameterItemViewModel> ToParameterViewModels()
    {
        if (Properties is not { Count: > 0 })
            return [];

        var required = Required is { Count: > 0 }
            ? new HashSet<string>(Required, StringComparer.Ordinal)
            : [];
        return
        [
            .. Properties.Select(property => new McpToolParameterItemViewModel(
                CreateParameterSignature(property.Key, property.Value, required.Contains(property.Key)),
                property.Value.Description))
        ];
    }

    private static string CreateParameterSignature(string name, JsonSchemaObject schema, bool isRequired)
    {
        var optionalMarker = isRequired ? "" : "?";
        var type = schema.FormatType();
        var defaultValue = schema.FormatDefaultValue();
        return defaultValue is null
            ? $"{name}{optionalMarker}: {type}"
            : $"{name}{optionalMarker}: {type} = {defaultValue}";
    }

    private string FormatType()
    {
        var type = FormatTypeCore();
        return Enum is { Count: > 0 }
            ? $"{type} [{string.Join(" | ", Enum.Select(FormatEnumValue))}]"
            : type;
    }

    private string FormatTypeCore()
    {
        if (Type.ValueKind is JsonValueKind.String)
            return FormatJsonSchemaType(Type.GetString());

        if (Type.ValueKind is JsonValueKind.Array)
        {
            var types = Type.EnumerateArray()
                .Where(static item => item.ValueKind is JsonValueKind.String)
                .Select(item => FormatJsonSchemaType(item.GetString()))
                .Where(static type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (types.Length is not 0)
                return string.Join(" | ", types);
        }

        var compositionTypes = EnumerateCompositionSchemas()
            .Select(static schema => schema.FormatType())
            .Where(static type => !string.IsNullOrWhiteSpace(type))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (compositionTypes.Length is not 0)
            return string.Join(" | ", compositionTypes);

        return string.IsNullOrWhiteSpace(Reference)
            ? "value"
            : Reference.Split('/').LastOrDefault() ?? "value";
    }

    private IEnumerable<JsonSchemaObject> EnumerateCompositionSchemas()
    {
        if (AnyOf is { Count: > 0 })
            foreach (var schema in AnyOf)
                yield return schema;

        if (OneOf is { Count: > 0 })
            foreach (var schema in OneOf)
                yield return schema;

        if (AllOf is { Count: > 0 })
            foreach (var schema in AllOf)
                yield return schema;
    }

    private string FormatJsonSchemaType(string? type) =>
        type switch
        {
            "array" when Items != null => $"array<{Items.FormatType()}>",
            "integer" => "int",
            { Length: > 0 } => type,
            _ => "value"
        };

    private string? FormatDefaultValue() => Default is { } defaultValue ? FormatJsonValue(defaultValue) : null;

    private static string FormatJsonValue(JsonElement value) => value.GetRawText();

    private static string FormatEnumValue(JsonElement value) =>
        value.ValueKind is JsonValueKind.String ? value.GetString() ?? "" : FormatJsonValue(value);
}
