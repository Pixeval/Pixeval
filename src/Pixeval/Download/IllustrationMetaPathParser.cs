#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationMetaPathParser.cs
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

using Pixeval.Download.MacroParser;
using Pixeval.Utilities;
using Pixeval.ViewModel;

namespace Pixeval.Download
{
    public class IllustrationMetaPathParser : IMetaPathParser<IllustrationViewModel>
    {
        private readonly MacroParser<IllustrationViewModel> _parser = new();

        public IllustrationMetaPathParser()
        {
            MacroProvider = new IllustrationMetaPathMacroProvider();
        }

        public IMetaPathMacroProvider<IllustrationViewModel> MacroProvider { get; }

        public string Reduce(string raw, IllustrationViewModel context)
        {
            _parser.SetupParsingEnvironment(new Lexer(raw));
            if (_parser.Parse() is { } root)
            {
                var result = root.Evaluate(MacroProvider, context);
                return result.IsNotNullOrBlank() ? result : throw new MacroParseException(MacroParserResources.ResultIsEmpty);
            }

            throw new MacroParseException(MacroParserResources.ResultIsEmpty);
        }
    }
}