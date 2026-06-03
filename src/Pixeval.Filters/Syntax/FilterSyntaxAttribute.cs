// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Filters.Syntax;

/// <summary>
/// 标记一个语法类型需要被对应上下文的过滤语言自动收集。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class FilterSyntaxAttribute<TContext> : Attribute;
