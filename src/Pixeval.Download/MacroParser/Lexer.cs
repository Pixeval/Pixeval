// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

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
