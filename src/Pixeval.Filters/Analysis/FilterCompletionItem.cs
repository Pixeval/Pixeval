// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 表示一条可用于自动补全的候选项。
/// </summary>
public sealed record FilterCompletionItem(
    string DisplayText,
    string InsertText,
    FilterTextSpan ReplacementSpan,
    string? Description = null,
    bool IsHintOnly = false);
