// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示字符串过滤值，保留原始文本和精确匹配标记。
/// </summary>
public sealed record FilterTextValue(string Source, FilterTextSpan ContentSpan, bool IsExact, FilterTextSpan Span) : FilterValue(Span)
{
    /// <summary>
    /// 返回当前字符串值对应的只读字符片段。
    /// </summary>
    public ReadOnlySpan<char> AsSpan() => ContentSpan.Slice(Source);

    /// <summary>
    /// 返回当前字符串值的文本内容。
    /// </summary>
    public override string ToString() => ContentSpan.GetText(Source);
}
