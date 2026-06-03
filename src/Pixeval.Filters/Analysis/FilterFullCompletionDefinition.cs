// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 表示只在全量补全时展示的静态补全定义，并可声明它代表的语法头前缀。
/// </summary>
public sealed record FilterFullCompletionDefinition(
    string Key,
    string DisplayText,
    string InsertText,
    string? Description = null,
    IReadOnlyList<string>? CoveredSyntaxPrefixes = null);
