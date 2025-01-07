// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Download.MacroParser.Ast;

public abstract record SingleNode<TContext> : IMetaPathNode<TContext>
{
    public abstract string Evaluate(IReadOnlyList<IMacro> env, TContext context);
}
