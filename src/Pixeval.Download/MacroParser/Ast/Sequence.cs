// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{First} {Remains}")]
public record Sequence<TContext>(SingleNode<TContext> First, Sequence<TContext>? Remains) : IMetaPathNode<TContext>
{
    public string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        var builder = new StringBuilder();
        for (var current = this; current is not null; current = current.Remains)
            _ = builder.Append(current.First.Evaluate(env, context));

        return builder.ToString();
    }
}
