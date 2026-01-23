// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;

namespace Pixeval.Download.MacroParser.Ast;

public interface IMetaPathNode<in TContext>
{
    string Evaluate(IReadOnlyList<IMacro> env, TContext context);
}
