// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Syntax;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 描述一次位于语法值内部的补全请求上下文。
/// </summary>
public sealed record FilterValueCompletionContext(
    FilterSyntaxMatch Match,
    string Source,
    FilterTextSpan TokenSpan,
    FilterTextSpan ValueSpan,
    FilterTextSpan FragmentSpan,
    bool IsNegated);
