// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Filters.Nodes;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 表示一次过滤分析的完整结果。
/// </summary>
public sealed record FilterAnalysisResult(FilterQuery? Query, IReadOnlyList<FilterDiagnostic> Diagnostics, IReadOnlyList<FilterCompletionItem> Completions)
{
    public static FilterAnalysisResult Empty { get; } = new(FilterQuery.Empty, [], []);

    public bool IsSuccess => Query is not null && Diagnostics.Count is 0;
}
