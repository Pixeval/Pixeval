// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{WhenTrue} : {WhenFalse}")]
public record ConditionalMacroBranches<TContext>(Sequence<TContext>? WhenTrue, Sequence<TContext>? WhenFalse) : IMetaPathNode<TContext>
{
    public string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        return string.Empty;
    }
}