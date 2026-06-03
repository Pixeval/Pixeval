// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("@{{{MacroName}{ConditionalBranches}}}")]
public record Macro(PlainText MacroName, ConditionalMacroBranches? ConditionalBranches) : SingleNode;
