// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("@({IsNot}){MacroName}={OptionalParameters}}}")]
public record Macro<TContext>(PlainText<TContext> MacroName, OptionalMacroParameter<TContext>? OptionalParameters, bool IsNot)
    : SingleNode<TContext>
{
    public override string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        var result = env.TryResolve(MacroName.Text, IsNot);
        return result switch
        {
            Unknown => throw new MacroParseException(MacroParseException.ErrorType.UnknownMacroName, MacroName.Text),
            ITransducer<TContext> when IsNot => throw new MacroParseException(MacroParseException.ErrorType.NegationNotAllowed, MacroName.Text),
            ITransducer<TContext> when OptionalParameters is not null => throw new MacroParseException(MacroParseException.ErrorType.NonParameterizedMacroBearingParameter, MacroName.Text),
            ITransducer<TContext> transducer => transducer.Substitute(context),
            IPredicate<TContext> when OptionalParameters is null => throw new MacroParseException(MacroParseException.ErrorType.ParameterizedMacroMissingParameter, MacroName.Text),
            IPredicate<TContext> predicate => predicate.Match(context) ^ predicate.IsNot ? OptionalParameters.Evaluate(env, context) : "",
            _ => throw new ArgumentOutOfRangeException(nameof(result))
        };
    }
}
