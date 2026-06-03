// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{Text}")]
public record PlainText(string Text, Range Position) : SingleNode;
