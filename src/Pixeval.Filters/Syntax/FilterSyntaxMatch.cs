// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;

namespace Pixeval.Filters.Syntax;

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

    public string DiagnosticText => HeaderText.Length > 0 ? HeaderText : Syntax.Key;

    public string CompletionText { get; } = Prefix + Alias + Suffix + (Syntax.ValueKind is FilterValueKind.None ? "" : ExampleValue ?? "");

    public string CompletionInsertText { get; } = Prefix + Alias + Suffix;
}
