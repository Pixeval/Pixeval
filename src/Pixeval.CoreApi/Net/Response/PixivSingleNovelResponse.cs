// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Diagnostics;
using System.Text.Json.Serialization;
using Pixeval.CoreApi.Model;

namespace Pixeval.CoreApi.Net.Response;

[DebuggerDisplay("{Novel}")]
[Factory]
public partial record PixivSingleNovelResponse
{
    [JsonPropertyName("novel")]
    public required Novel Novel { get; set; }
}
