// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Filters;

/// <summary>
/// 表示一个语法项最终作用于筛选条件还是视图范围。
/// </summary>
public enum FilterTermRole
{
    Predicate,
    ViewRange
}

/// <summary>
/// 表示语法项期望读取的值类型。
/// </summary>
public enum FilterValueKind
{
    None,
    Text,
    LongRange,
    DoubleRange,
    Date
}

/// <summary>
/// 表示整数范围绑定时采用的语义模式。
/// </summary>
public enum FilterLongRangeBindingMode
{
    Inclusive,
    OneBasedIndex
}

/// <summary>
/// 描述一个语法项可接受的前缀、别名和示例值。
/// </summary>
[DebuggerDisplay("{Prefix}{Aliases[0]}{Suffix}{(Syntax.ValueKind is FilterValueKind.None ? \"\" : ExampleValue ?? \"\");} ({Description})")]
public sealed record FilterSyntaxPattern(
    string Prefix,
    IReadOnlyList<string> Aliases,
    string Suffix = "",
    object? Metadata = null,
    string? ExampleValue = null,
    string? Description = null)
{
    /// <summary>
    /// 创建默认的无前缀语法模式。
    /// </summary>
    public static FilterSyntaxPattern Default(string? exampleValue = null, string? description = null)
        => new("", [""], "", null, exampleValue, description);

    /// <summary>
    /// 创建仅带前缀的语法模式。
    /// </summary>
    public static FilterSyntaxPattern PrefixOnly(string prefix, string? exampleValue = null, string? description = null)
        => new(prefix, [""], "", null, exampleValue, description);

    /// <summary>
    /// 创建带关键字和后缀的语法模式。
    /// </summary>
    public static FilterSyntaxPattern Keyword(string keyword, string suffix = ":", string? exampleValue = null, string? description = null)
        => new("", [keyword], suffix, null, exampleValue, description);

    /// <summary>
    /// 将模式展开为可直接匹配的语法项集合。
    /// </summary>
    internal IEnumerable<FilterSyntaxMatch> Expand(FilterSyntax syntax)
    {
        if (Aliases.Count is 0)
        {
            yield return new(syntax, Prefix, "", Suffix, Metadata, ExampleValue ?? syntax.ExampleValue, Description);
            yield break;
        }

        foreach (var alias in Aliases)
            yield return new(syntax, Prefix, alias, Suffix, Metadata, ExampleValue ?? syntax.ExampleValue, Description);
    }
}

/// <summary>
/// 表示展开后的单个可匹配语法头。
/// </summary>
[DebuggerDisplay("{CompletionText} ({Description})")]
public sealed record FilterSyntaxMatch(
    FilterSyntax Syntax,
    string Prefix,
    string Alias,
    string Suffix,
    object? Metadata,
    string? ExampleValue,
    string? Description)
{
    public string HeaderText { get; } = Prefix + Alias + Suffix;

    public string CompletionText { get; } = Prefix + Alias + Suffix + (Syntax.ValueKind is FilterValueKind.None ? "" : ExampleValue ?? "");
}

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
            rawValue.Span.Length > 0 ? rawValue.Span : termSpan);
        return false;
    }

    /// <summary>
    /// 由具体语法实现真实的值绑定逻辑。
    /// </summary>
    protected abstract bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic);
}

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
        diagnostic = new(FilterDiagnosticKind.InternalExpectedTextValue, rawValue.Span);
        return false;
    }
}

/// <summary>
/// 表示读取整数范围值的语法。
/// </summary>
public abstract class FilterLongRangeSyntax : FilterSyntax
{
    protected abstract FilterLongRangeBindingMode BindingMode { get; }

    public sealed override FilterValueKind ValueKind => FilterValueKind.LongRange;

    /// <summary>
    /// 按当前绑定模式将原始整数范围转换为具体语义对象。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawLongRangeValue range)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedLongRangeValue, rawValue.Span);
            return false;
        }

        switch (BindingMode)
        {
            case FilterLongRangeBindingMode.Inclusive:
                if (FilterLongRange.TryCreate(range.Value, rawValue.Span, out var inclusiveRange, out diagnostic))
                {
                    value = inclusiveRange;
                    return true;
                }

                value = null;
                return false;
            case FilterLongRangeBindingMode.OneBasedIndex:
                if (FilterLongRange.TryCreateIndexRange(range.Value, rawValue.Span, out var viewRange, out diagnostic))
                {
                    value = viewRange;
                    return true;
                }

                value = null;
                return false;
            default:
                value = null;
                diagnostic = new(FilterDiagnosticKind.InternalUnsupportedLongRangeBindingMode, rawValue.Span);
                return false;
        }
    }
}

/// <summary>
/// 表示读取小数范围值的语法。
/// </summary>
public abstract class FilterDoubleRangeSyntax : FilterSyntax
{
    public sealed override FilterValueKind ValueKind => FilterValueKind.DoubleRange;

    /// <summary>
    /// 将原始小数范围绑定为具体小数范围语义。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawDoubleRangeValue range)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedDoubleRangeValue, rawValue.Span);
            return false;
        }

        if (FilterDoubleRange.TryCreate(range.Value, rawValue.Span, out var decimalRange, out diagnostic))
        {
            value = decimalRange;
            return true;
        }

        value = null;
        return false;
    }
}

/// <summary>
/// 表示读取日期值的语法。
/// </summary>
public abstract class FilterDateSyntax : FilterSyntax
{
    protected virtual int FallbackYear => DateTime.UtcNow.Year;

    public sealed override FilterValueKind ValueKind => FilterValueKind.Date;

    /// <summary>
    /// 将原始日期值绑定为具体时间点。
    /// </summary>
    protected sealed override bool TryBindCore(FilterSyntaxMatch match, FilterValue rawValue, out object? value, out FilterDiagnostic? diagnostic)
    {
        if (rawValue is not FilterRawDateValue date)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InternalExpectedDateValue, rawValue.Span);
            return false;
        }

        try
        {
            value = date.Value.ToDateTimeOffset(FallbackYear);
            diagnostic = null;
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            value = null;
            diagnostic = new(FilterDiagnosticKind.InvalidDate, rawValue.Span, date.Value);
            return false;
        }
    }
}

/// <summary>
/// 标记一个语法类型需要被对应上下文的过滤语言自动收集。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class FilterSyntaxAttribute<TContext> : Attribute;
