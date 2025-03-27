// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Mako.Net.Request;

public record RestrictedModeSettingsRequest([property: JsonPropertyName("is_restricted_mode_enabled")] bool IsRestrictedModeEnabled);
