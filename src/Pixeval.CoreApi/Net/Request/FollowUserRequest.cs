// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Mako.Global.Enum;

namespace Mako.Net.Request;

public record FollowUserRequest([property: JsonPropertyName("user_id")] long Id, [property: JsonPropertyName("restrict")] PrivacyPolicy Restrict);
