// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Filters.Analysis;

/// <summary>
/// 根据当前语法值上下文提供外部补全项。
/// </summary>
public delegate IReadOnlyList<FilterCompletionDefinition>? FilterValueCompletionProvider(FilterValueCompletionContext context);
