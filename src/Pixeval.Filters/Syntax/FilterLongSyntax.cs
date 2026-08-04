// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取整数值的语法。
/// </summary>
public abstract class FilterLongSyntax<TContext> : FilterSyntax<TContext, long>
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.Long;

    /// <summary>
    /// 将原始整数值绑定为 long。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out long value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is FilterRawLongValue longValue)
        {
            value = longValue.Value;
            diagnostic = null;
            return true;
        }

        value = 0;
        diagnostic = new(FilterDiagnosticKind.InternalExpectedLongValue, rawValue.Span, match.DiagnosticText);
        return false;
    }
}
