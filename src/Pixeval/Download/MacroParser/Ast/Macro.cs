using System;
using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Download.MacroParser.Ast
{
    public record Macro<TContext>(PlainText<TContext> MacroName, OptionalMacroParameter<TContext>? OptionalParameters)
        : SingleNode<TContext>
    {
        public override string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context)
        {
            switch (env.TryResolve(MacroName.Text))
            {
                case IMacro<TContext>.ITransducer transducer:
                    ThrowHelper.ThrowIf<IllegalMacroException>(OptionalParameters is not null, MacroParserResources.NonParameterizedMacroBearingParameterFormatted.Format(MacroName));
                    return transducer.Substitute(context);
                case IMacro<TContext>.IPredicate predicate:
                    if (predicate.Match(context))
                    {
                        return OptionalParameters?.Evaluate(env, context) ?? ThrowHelper.ThrowException<IllegalMacroException, string>(MacroParserResources.ParameterizedMacroMissingParameterFormatted.Format(MacroName));
                    }

                    return string.Empty;
                case IMacro<TContext>.Unknown:
                    return ThrowHelper.ThrowException<IllegalMacroException, string>(MacroParserResources.UnknownMacroNameFormatted.Format(MacroName));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}