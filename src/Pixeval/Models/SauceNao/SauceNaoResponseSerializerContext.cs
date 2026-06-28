// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Text.Json.Serialization;

namespace Pixeval.Models.SauceNao;

[JsonSerializable(typeof(SauceNaoResponse))]
internal partial class SauceNaoResponseSerializerContext : JsonSerializerContext;
