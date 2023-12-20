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

using Pixeval.Util;
using Pixeval.Utilities;

namespace Pixeval.Download.MacroParser.Ast;

public record Macro<TContext>(PlainText<TContext> MacroName, OptionalMacroParameter<TContext>? OptionalParameters)
    : SingleNode<TContext>
{
    public override string Evaluate(IMetaPathMacroProvider<TContext> env, TContext context)
    {
        var result = env.TryResolve(MacroName.Text);
        switch (result)
        {
            case IMacro<TContext>.ITransducer transducer:
                ThrowHelper.ThrowIf<IllegalMacroException>(OptionalParameters is not null, MacroParserResources.NonParameterizedMacroBearingParameterFormatted.Format(MacroName));
                return transducer.Substitute(context);
            case IMacro<TContext>.IPredicate predicate:
                if (predicate.Match(context))
                {
                    return OptionalParameters?.Evaluate(env, context) ?? ThrowHelper.ThrowException<IllegalMacroException, string>(MacroParserResources.ParameterizedMacroMissingParameterFormatted.Format(MacroName));
                }

                return "";
            case IMacro<TContext>.Unknown:
                return ThrowHelper.ThrowException<IllegalMacroException, string>(MacroParserResources.UnknownMacroNameFormatted.Format(MacroName));
            default:
                return WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<IMacro<TContext>, string>(result);
        }
    }
}
