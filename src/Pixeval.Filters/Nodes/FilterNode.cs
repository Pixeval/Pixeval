// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Nodes;

/// <summary>
/// 表示过滤语法树中的一个节点。
/// </summary>
public abstract record FilterNode(FilterTextSpan Span, bool IsNegated);
