// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;
using Pixeval.CoreApi.Global.Enum;

namespace Pixeval.CoreApi.Net.Request;

public record FollowUserRequest([property: JsonPropertyName("user_id")] long Id, [property: JsonPropertyName("restrict")] PrivacyPolicy Restrict);
