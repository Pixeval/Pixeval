// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示一个语法项最终作用于筛选条件还是视图范围。
/// </summary>
public enum FilterTermRole
{
    Predicate,
    ViewRange
}
