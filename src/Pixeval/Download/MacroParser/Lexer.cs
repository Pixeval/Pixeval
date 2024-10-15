#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/Lexer.cs
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

using System.IO;
using System.Linq;
using Pixeval.Utilities;

namespace Pixeval.Download.MacroParser;

public class Lexer(string rawString)
{
    private readonly CharStream _rawString = new(rawString);

    private char CurrentChar => _rawString.Peek();

    public TokenInfo? NextToken()
    {
        switch (CurrentChar)
        {
            case char.MaxValue:
                return null;
            case '@':
                var at = new TokenInfo(TokenKind.At, "@", _rawString.Forward..(_rawString.Forward + 1));
                _rawString.Advance();
                return at;
            case '!':
                var exclamation = new TokenInfo(TokenKind.Exclamation, "!", _rawString.Forward..(_rawString.Forward + 1));
                _rawString.Advance();
                return exclamation;
            case '{':
                var lBrace = new TokenInfo(TokenKind.LBrace, "{", _rawString.Forward..(_rawString.Forward + 1));
                _rawString.Advance();
                return lBrace;
            case '}':
                var rBrace = new TokenInfo(TokenKind.RBrace, "}", _rawString.Forward..(_rawString.Forward + 1));
                _rawString.Advance();
                return rBrace;
            case '=':
                var equal = new TokenInfo(TokenKind.Equal, "=", _rawString.Forward..(_rawString.Forward + 1));
                _rawString.Advance();
                return equal;
            default:
                return PlainText();
        }
    }

    private TokenInfo? PlainText()
    {
        var forward = _rawString.Forward;
        _rawString.AdvanceMarker();
        var str = _rawString.GetUntilIf(ch => ch is not '{' and not '}' and not '@' and not '=');
        var token = new TokenInfo(TokenKind.PlainText, str, forward..(forward + str.Length));
        return Path.GetInvalidPathChars().Intersect(token.Text).IsNotNullOrEmpty()
            ? null
            : token;
    }
}
