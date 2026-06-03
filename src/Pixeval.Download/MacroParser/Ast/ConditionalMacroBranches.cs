// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("? {WhenTrue} : {WhenFalse}")]
public record ConditionalMacroBranches(Sequence? WhenTrue, Sequence? WhenFalse) : IMetaPathNode;
