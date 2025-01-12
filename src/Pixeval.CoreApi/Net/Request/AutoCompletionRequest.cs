// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Request;

public class AutoCompletionRequest(string word)
{
    [JsonPropertyName("merge_plain_keyword_results")]
    public bool MergePlainKeywordResult { get; } = true;

    [JsonPropertyName("word")]
    public string Word { get; } = word;
}
