// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{Text}")]
public record PlainText<TContext>(string Text, Range Position) : SingleNode<TContext>
{
    public override string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        return Text;
    }
}
