// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Filters.Text;

namespace Pixeval.Filters.Values;

/// <summary>
/// 表示语法解析阶段产出的原始值对象。
/// </summary>
public abstract record FilterValue(FilterTextSpan Span);
