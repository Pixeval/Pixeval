// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示不需要附加值的布尔语法。
/// </summary>
public abstract class FilterFlagSyntax : FilterSyntax
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.None;

    /// <summary>
    /// 直接返回当前模式携带的布尔元数据。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        value = match.Metadata ?? true;
        diagnostic = null;
        return true;
    }
}
