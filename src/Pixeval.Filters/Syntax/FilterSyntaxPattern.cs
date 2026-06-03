// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Filters.Syntax;

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
