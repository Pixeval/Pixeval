// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 表示由外部注入到过滤语言中的静态补全定义。
/// </summary>
public sealed record FilterCompletionDefinition(string Key, string DisplayText, string InsertText, string? Description = null);
