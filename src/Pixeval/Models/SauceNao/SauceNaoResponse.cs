// Copyright (c) Mako.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

internal record SauceNaoResponse
{
    [JsonPropertyName("header")]
    public required SauceNaoResponseHeader Header { get; set; }

    [JsonPropertyName("results")]
    public IReadOnlyList<SauceNaoResult> Results { get; set; } = [];
}
