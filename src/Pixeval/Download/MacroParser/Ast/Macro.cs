// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Diagnostics;
using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Download.MacroParser.Ast;

[DebuggerDisplay("@({IsNot}){MacroName}={OptionalParameters}}}")]
public record Macro<TContext>(PlainText<TContext> MacroName, OptionalMacroParameter<TContext>? OptionalParameters, bool IsNot)
    : SingleNode<TContext>
{
    public override string Evaluate(IReadOnlyList<IMacro> env, TContext context)
    {
        var result = env.TryResolve(MacroName.Text, IsNot);
        return (result) switch
        {
            Unknown => ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.UnknownMacroNameFormatted.Format(MacroName))),
            ITransducer<TContext> when IsNot => ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.NegationNotAllowedFormatted.Format(MacroName))),
            ITransducer<TContext> when OptionalParameters is not null => ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.NonParameterizedMacroBearingParameterFormatted.Format(MacroName))),
            ITransducer<TContext> transducer => transducer.Substitute(context),
            IPredicate<TContext> when OptionalParameters is null => ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.ParameterizedMacroMissingParameterFormatted.Format(MacroName))),
            IPredicate<TContext> predicate => predicate.Match(context) ^ predicate.IsNot ? OptionalParameters.Evaluate(env, context) : "",
            _ => ThrowHelper.ArgumentOutOfRange<IMacro, string>(result)
        };
    }
}
