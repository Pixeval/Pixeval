// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Filters.Analysis;
using Pixeval.Filters.Text;
using Pixeval.Filters.Values;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 表示一个可由外部注册的过滤语法定义。
/// </summary>
public abstract class FilterSyntax
{
    public abstract string Key { get; }

    public virtual FilterTermRole Role => FilterTermRole.Predicate;

    public abstract FilterValueKind ValueKind { get; }

    public abstract IReadOnlyList<FilterSyntaxPattern> Patterns { get; }

    public virtual string? ExampleValue => null;

    /// <summary>
    /// 将原始值绑定为当前语法对应的语义对象。
    /// </summary>
    internal bool TryBind(FilterSyntaxMatch match, FilterValue rawValue, FilterTextSpan termSpan, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (TryBindCore(match, rawValue, out value, out diagnostic))
        {
            diagnostic = null;
            return true;
        }

        diagnostic ??= new(
            FilterDiagnosticKind.InvalidValue,
            rawValue.Span.Length > 0 ? rawValue.Span : termSpan,
            match.DiagnosticText);
        return false;
    }

    /// <summary>
    /// 由具体语法实现真实的值绑定逻辑。
    /// </summary>
    protected abstract bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic);
}
