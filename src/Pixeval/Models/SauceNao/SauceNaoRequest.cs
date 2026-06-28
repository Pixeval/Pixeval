// Copyright (c) Mako.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

public class SauceNaoRequest(string apiKey)
{
    [JsonPropertyName("api_key")]
    public string ApiKey { get; } = apiKey;

    public SauceNaoIndex Index { get; init; } = SauceNaoIndex.All;

    [JsonPropertyName("output_type")]
    public OutputTypeMode OutputType { get; init; } = OutputTypeMode.Json;

    [JsonPropertyName("numres")]
    public int NumberResult { get; init; }

    [JsonPropertyName("dedupe")]
    public DedupeMode Dedupe { get; init; } = DedupeMode.AllImplementedDedupeMethods;

    [JsonPropertyName("hide")]
    public HideMode Hide { get; init; } = HideMode.ShowAll;

    public enum OutputTypeMode
    {
        Html,
        Xml,
        Json
    }

    public enum DedupeMode
    {
        NoResultDeduping,
        ConsolidateBooruResultsAndDedupeByItemIdentifier,
        AllImplementedDedupeMethods
    }

    public enum HideMode
    {
        ShowAll,
        HideExpectedExplicit,
        HideExpectedAndSuspectedExplicit,
        HideAllButExpectedSafe
    }

    public string ToQueryString()
    {
        var sb = new StringBuilder(
            "https://saucenao.com/search.php"
            + $"?api_key={ApiKey}"
            + $"&output_type={OutputType:D}"
            + (Index is SauceNaoIndex.All ? $"&db={Index:D}" : $"&dbmask={Index:D}"));
        if (NumberResult is not 0)
            _ = sb.Append($"&numres={NumberResult}");
        if (Dedupe is not DedupeMode.AllImplementedDedupeMethods)
            _ = sb.Append($"&dedupe={Dedupe:D}");
        if (Hide is not HideMode.ShowAll)
            _ = sb.Append($"&hide={Hide:D}");

        return sb.ToString();
    }
}
