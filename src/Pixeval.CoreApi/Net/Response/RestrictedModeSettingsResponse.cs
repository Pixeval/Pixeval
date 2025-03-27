// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Response;

[Factory]
public partial record RestrictedModeSettingsResponse
{
    [JsonPropertyName("is_restricted_mode_enabled")]
    public required bool IsRestrictedModeEnabled { get; set; }
}
