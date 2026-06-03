// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示一个绑定到具体语法的谓词节点。
/// </summary>
public sealed record FilterPredicateNode(FilterSyntax Syntax, object? Value, FilterTextSpan Span, bool IsNegated = false)
    : FilterNode(Span, IsNegated);
