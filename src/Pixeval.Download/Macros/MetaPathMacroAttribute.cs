// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.Macros;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MetaPathMacroAttribute<TContext> : Attribute;
