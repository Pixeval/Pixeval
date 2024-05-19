#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationMetaPathParser.cs
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
using Pixeval.Controls;
using Pixeval.Download.MacroParser;
using Pixeval.Download.Macros;
using Pixeval.Utilities;

namespace Pixeval.Download;

public class IllustrationMetaPathParser : IMetaPathParser<IllustrationItemViewModel>
{
    private readonly MacroParser<IllustrationItemViewModel> _parser = new();

    private static readonly IReadOnlyList<IMacro> _macroProviderStatic = MetaPathMacroAttributeHelper.GetIWorkViewModelInstances();
 
    public IReadOnlyList<IMacro> MacroProvider => _macroProviderStatic;

    public string Reduce(string raw, IllustrationItemViewModel context)
    {
        _parser.SetupParsingEnvironment(new Lexer(raw));
        if (_parser.Parse() is { } root)
        {
            var result = root.Evaluate(MacroProvider, context);
            if (result.IsNotNullOrBlank())
                return result;
        }

        return ThrowUtils.MacroParse<string>(MacroParserResources.ResultIsEmpty);
    }
}
