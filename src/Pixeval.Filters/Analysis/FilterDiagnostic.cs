// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using Pixeval.Filters.Text;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 表示一条带位置的结构化诊断信息。
/// </summary>
public sealed record FilterDiagnostic(
    FilterDiagnosticKind Kind,
    FilterTextSpan Span,
    params IReadOnlyList<object?> Arguments);
