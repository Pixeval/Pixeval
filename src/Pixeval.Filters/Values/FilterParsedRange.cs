// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示尚未绑定语义的通用范围值。
/// </summary>
public readonly record struct FilterParsedRange<T>(T? Start, bool IsStartInclusive, T? End, bool IsEndInclusive)
    where T : struct;
