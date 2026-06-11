// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("@{{{MacroName}:{Formatter}{ConditionalBranches}}}")]
public record Macro(PlainText MacroName, PlainText? Formatter, ConditionalMacroBranches? ConditionalBranches) : SingleNode;
