// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取小数值的语法。
/// </summary>
public abstract class FilterDoubleSyntax<TContext> : FilterSyntax<TContext, double>
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.Double;

    /// <summary>
    /// 将原始小数值绑定为 double。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out double value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is FilterRawDoubleValue doubleValue)
        {
            value = doubleValue.Value;
            diagnostic = null;
            return true;
        }

        value = 0;
        diagnostic = new(FilterDiagnosticKind.InternalExpectedDoubleValue, rawValue.Span, match.DiagnosticText);
        return false;
    }
}
