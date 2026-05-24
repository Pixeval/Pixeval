// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Filters;

/// <summary>
/// 表示过滤文本中的一个连续区间，用于高亮和诊断定位。
/// </summary>
public readonly record struct FilterTextSpan(int Start, int Length)
{
    /// <summary>
    /// 获取区间的结束位置（不含尾端）。
    /// </summary>
    public int End => Start + Math.Max(Length, 0);

    /// <summary>
    /// 创建一个位于指定位置的空区间。
    /// </summary>
    public static FilterTextSpan EmptyAt(int position) => new(Math.Max(position, 0), 0);

    /// <summary>
    /// 根据起止边界创建区间。
    /// </summary>
    public static FilterTextSpan FromBounds(int start, int end)
        => new(Math.Max(start, 0), Math.Max(0, end - start));

    /// <summary>
    /// 从源字符串中切出当前区间的字符片段。
    /// </summary>
    public ReadOnlySpan<char> Slice(string source)
    {
        var safeStart = Math.Clamp(Start, 0, source.Length);
        var safeLength = Math.Clamp(Length, 0, source.Length - safeStart);
        return source.AsSpan(safeStart, safeLength);
    }

    /// <summary>
    /// 将当前区间对应的文本转换为字符串。
    /// </summary>
    public string GetText(string source) => new(Slice(source));
}