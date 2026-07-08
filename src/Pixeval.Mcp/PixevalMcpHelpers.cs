// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Globalization;
using Mako.Engine.Implements;
using Mako.Global.Enum;
using Mako.Model;
using Pixeval.Mcp.Dtos;

namespace Pixeval.Mcp;

internal static class PixevalMcpHelpers
{
    public const int DefaultCount = 20;
    public const int MaxCount = 100;

    public static int ClampCount(int count) => int.Clamp(count, 1, MaxCount);

    public static PixevalWorkListDto CreateWorkListDto(
        IPixevalMcpRuntime runtime,
        IReadOnlyList<WorkBase> works,
        string? workFilter)
    {
        if (string.IsNullOrWhiteSpace(workFilter))
        {
            var unfiltered = works.Select(PixevalWorkDto.FromWork).ToArray();
            return new(unfiltered.Length, unfiltered);
        }

        var analysis = runtime.AnalyzeWorkFilter(workFilter, -1);
        if (!analysis.IsSuccess)
            return new(0, [], analysis);

        var filtered = runtime.FilterWorks(works, workFilter).Select(PixevalWorkDto.FromWork).ToArray();
        return new(filtered.Length, filtered, analysis);
    }

    public static string GetImageMimeType(string url)
    {
        var path = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.AbsolutePath : url;
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    public static IReadOnlyList<string> GetOriginalImagePages(Illustration illustration) =>
        illustration.PageCount <= 1
            ? [illustration.OriginalSingleUrl ?? illustration.MetaSinglePage.OriginalImageUrl ?? ""]
            : [.. illustration.MetaPages.Select(static t => t.OriginalUrl)];

    public static IllustrationSearchArguments CreateIllustrationSearchArguments(
        string query,
        SearchIllustrationTagMatchOption match,
        WorkSortOption sort,
        bool includeAi,
        SearchIllustrationContentType contentType,
        SearchIllustrationRatioPattern ratioPattern,
        string? startDate,
        string? endDate,
        bool mergePlainKeywordResults,
        bool includeTranslatedTagResults,
        bool includePotentialViolationWorks,
        int? widthMin,
        int? widthMax,
        int? heightMin,
        int? heightMax,
        string? tool)
    {
        ValidateRange(widthMin, widthMax, nameof(widthMin), nameof(widthMax));
        ValidateRange(heightMin, heightMax, nameof(heightMin), nameof(heightMax));

        return new(query)
        {
            MatchOption = match,
            SortOption = sort,
            AiType = includeAi,
            ContentType = contentType,
            RatioPattern = ratioPattern,
            StartDate = ParseDate(startDate, nameof(startDate)),
            EndDate = ParseDate(endDate, nameof(endDate)),
            MergePlainKeywordResults = mergePlainKeywordResults,
            IncludeTranslatedTagResults = includeTranslatedTagResults,
            IncludePotentialViolationWorks = includePotentialViolationWorks,
            WidthMin = widthMin,
            WidthMax = widthMax,
            HeightMin = heightMin,
            HeightMax = heightMax,
            Tool = string.IsNullOrWhiteSpace(tool) ? null : tool
        };
    }

    public static NovelSearchArguments CreateNovelSearchArguments(
        string query,
        SearchNovelTagMatchOption match,
        WorkSortOption sort,
        bool includeAi,
        string? language,
        SearchNovelContentLengthOption contentLengthOption,
        int? contentLengthMin,
        int? contentLengthMax,
        bool originalOnly,
        int? genreId,
        bool replaceableOnly,
        string? startDate,
        string? endDate,
        bool mergePlainKeywordResults,
        bool includeTranslatedTagResults,
        bool includePotentialViolationWorks)
    {
        ValidateRange(contentLengthMin, contentLengthMax, nameof(contentLengthMin), nameof(contentLengthMax));

        return new(query)
        {
            MatchOption = match,
            SortOption = sort,
            AiType = includeAi,
            LangCode = string.IsNullOrWhiteSpace(language) ? null : language,
            Option = contentLengthOption,
            ContentLengthMin = contentLengthMin,
            ContentLengthMax = contentLengthMax,
            IsOriginalOnly = originalOnly,
            GenreId = genreId,
            IsReplaceableOnly = replaceableOnly,
            StartDate = ParseDate(startDate, nameof(startDate)),
            EndDate = ParseDate(endDate, nameof(endDate)),
            MergePlainKeywordResults = mergePlainKeywordResults,
            IncludeTranslatedTagResults = includeTranslatedTagResults,
            IncludePotentialViolationWorks = includePotentialViolationWorks
        };
    }

    private static DateTimeOffset? ParseDate(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateOnly.TryParseExact(
                value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
            return new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        throw new PixevalMcpException($"{parameterName} must be a date in yyyy-MM-dd form.");
    }

    private static void ValidateRange(int? min, int? max, string minName, string maxName)
    {
        if (min is < 0)
            throw new PixevalMcpException($"{minName} must be greater than or equal to 0.");
        if (max is < 0)
            throw new PixevalMcpException($"{maxName} must be greater than or equal to 0.");
        if (min > max)
            throw new PixevalMcpException($"{minName} must be less than or equal to {maxName}.");
    }
}
