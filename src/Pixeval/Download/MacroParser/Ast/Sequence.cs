// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{First} {Remains}")]
public record Sequence<TContext>(SingleNode<TContext> First, Sequence<TContext>? Remains) : IMetaPathNode<TContext>
{
    public string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        return First.Evaluate(env, context) + (Remains?.Evaluate(env, context) ?? string.Empty);
    }
}
