// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Analysis;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示读取字符串值的语法。
/// </summary>
public abstract class FilterTextSyntax : FilterSyntax
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.Text;

    /// <summary>
    /// 将原始字符串值绑定为文本语义。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is FilterTextValue text)
        {
            value = text;
            diagnostic = null;
            return true;
        }

        value = null;
        diagnostic = new(FilterDiagnosticKind.InternalExpectedTextValue, rawValue.Span, match.DiagnosticText);
        return false;
    }
}
