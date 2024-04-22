#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/Macro.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using Pixeval.Utilities;
using WinUI3Utilities;

namespace Pixeval.Download.MacroParser.Ast;

public record Macro<TContext>(PlainText<TContext> MacroName, OptionalMacroParameter<TContext>? OptionalParameters)
    : SingleNode<TContext>
{
    public override string Evaluate(IMetaPathMacroProvider env, TContext context)
    {
        var result = env.TryResolve(MacroName.Text);
        switch (result)
        {
            case ITransducer<TContext> transducer:
                return OptionalParameters is not null
                    ? ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.NonParameterizedMacroBearingParameterFormatted.Format(MacroName)))
                    : transducer.Substitute(context);
            case IPredicate<TContext> predicate:
                if (predicate.Match(context))
                {
                    return OptionalParameters?.Evaluate(env, context) ?? ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.ParameterizedMacroMissingParameterFormatted.Format(MacroName)));
                }

                return "";
            case Unknown:
                return ThrowUtils.Throw<string>(new IllegalMacroException(MacroParserResources.UnknownMacroNameFormatted.Format(MacroName)));
            default:
                return ThrowHelper.ArgumentOutOfRange<IMacro, string>(result);
        }
    }
}
