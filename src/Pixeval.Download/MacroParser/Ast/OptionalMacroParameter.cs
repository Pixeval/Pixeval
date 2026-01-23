// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("{Content}")]
public record OptionalMacroParameter<TContext>(Sequence<TContext> Content) : IMetaPathNode<TContext>
{
    public string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        return Content.Evaluate(env, context);
    }
}
