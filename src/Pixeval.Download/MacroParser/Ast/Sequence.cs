// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{First} {Remains}")]
public record Sequence(SingleNode First, Sequence? Remains) : IMetaPathNode;
