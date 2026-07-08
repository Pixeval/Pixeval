// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Mako.Global.Enum;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace Pixeval.Mcp;

internal static class PixevalMcpResult
{
    public static JsonSerializerOptions JsonOptions { get; } = CreateJsonOptions();

    public static CallToolResult Success<T>(T value)
    {
        var type = value?.GetType() ?? typeof(T);
        var jsonTypeInfo = GetJsonTypeInfo(type);
        var text = JsonSerializer.Serialize(value, jsonTypeInfo);
        return new()
        {
            Content = [new TextContentBlock { Text = text }],
            StructuredContent = JsonSerializer.SerializeToElement(value, jsonTypeInfo)
        };
    }

    public static CallToolResult Error(string message) =>
        new()
        {
            IsError = true,
            Content = [new TextContentBlock { Text = message }]
        };

    public static string Json<T>(T value)
    {
        var type = value?.GetType() ?? typeof(T);
        return JsonSerializer.Serialize(value, GetJsonTypeInfo(type));
    }

    private static JsonTypeInfo GetJsonTypeInfo(Type type) => JsonOptions.GetTypeInfo(type);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions);
        options.TypeInfoResolverChain.Insert(0, PixevalMcpJsonContext.Default);
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.WriteIndented = true;
        AddEnumConverters(options);
        return options;
    }

    private static void AddEnumConverters(JsonSerializerOptions options)
    {
        AddEnumConverter<PixevalHelpTopic>(options);
        AddEnumConverter<WorkType>(options);
        AddEnumConverter<SimpleWorkType>(options);
        AddEnumConverter<PrivacyPolicy>(options);
        AddEnumConverter<RankOption>(options);
        AddEnumConverter<WorkSortOption>(options);
        AddEnumConverter<SearchIllustrationTagMatchOption>(options);
        AddEnumConverter<SearchIllustrationContentType>(options);
        AddEnumConverter<SearchIllustrationRatioPattern>(options);
        AddEnumConverter<SearchNovelTagMatchOption>(options);
        AddEnumConverter<SearchNovelContentLengthOption>(options);
        AddEnumConverter<PixevalHistoryType>(options);
        AddEnumConverter<PixevalDownloadAction>(options);
        AddEnumConverter<PixevalWorkSubscriptionType>(options);
        AddEnumConverter<PixevalWorkSubscriptionWorkKind>(options);
    }

    private static void AddEnumConverter<TEnum>(JsonSerializerOptions options)
        where TEnum : struct, Enum =>
        options.Converters.Insert(0, new JsonStringEnumConverter<TEnum>(JsonNamingPolicy.SnakeCaseLower, false));
}
