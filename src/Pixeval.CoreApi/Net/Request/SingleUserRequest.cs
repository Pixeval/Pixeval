// Copyright (c) Pixeval.CoreApi.
// Licensed under the GPL v3 License.

using System.Text.Json.Serialization;

namespace Pixeval.CoreApi.Net.Request;

public record SingleUserRequest([property: JsonPropertyName("user_id")] long Id, [property: JsonPropertyName("filter")] string Filter);
