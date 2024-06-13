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
