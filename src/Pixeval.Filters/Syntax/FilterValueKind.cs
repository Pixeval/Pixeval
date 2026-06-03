// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示语法项期望读取的值类型。
/// </summary>
public enum FilterValueKind
{
    None,
    Text,
    Long,
    Double,
    LongRange,
    DoubleRange,
    Date
}
