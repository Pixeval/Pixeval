// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("@{{{MacroName}?{ConditionalBranches}}}")]
public record Macro<TContext>(PlainText<TContext> MacroName, ConditionalMacroBranches<TContext>? ConditionalBranches)
    : SingleNode<TContext>
{
    public override string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        var result = env.TryResolve(MacroName.Text);
        return result switch
        {
            Unknown => throw new MacroParseException(MacroParseException.ErrorType.UnknownMacroName, MacroName.Text, MacroName.Position.ToTextSpan()),
            ITransducer<TContext> when ConditionalBranches is not null => throw new MacroParseException(MacroParseException.ErrorType.NonParameterizedMacroBearingParameter, MacroName.Text, MacroName.Position.ToTextSpan()),
            ITransducer<TContext> transducer => transducer.Substitute(context),
            IPredicate<TContext> when ConditionalBranches is null => throw new MacroParseException(MacroParseException.ErrorType.ParameterizedMacroMissingParameter, MacroName.Text, MacroName.Position.ToTextSpan()),
            IPredicate<TContext> predicate => predicate.Match(context)
                ? ConditionalBranches.WhenTrue?.Evaluate(env, context) ?? ""
                : ConditionalBranches.WhenFalse?.Evaluate(env, context) ?? "",
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }
}
