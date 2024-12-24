#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/MacroParser.cs
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

using Pixeval.Download.MacroParser.Ast;
using Pixeval.Utilities;

namespace Pixeval.Download.MacroParser;

public class MacroParser<TContext>
{
    private TokenInfo? _currentToken;
    private bool _expectContextualEqual;
    private Lexer? _lexer;

    public void SetupParsingEnvironment(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
        _expectContextualEqual = false;
    }

    private TokenInfo EatToken(TokenKind kind)
    {
        if (_currentToken is { TokenKind: var tokenKind } token && tokenKind == kind)
        {
            _currentToken = _lexer!.NextToken();
            return token;
        }

        return ThrowUtils.MacroParse<TokenInfo>(MacroParserResources.UnexpectedTokenFormatted.Format((object?)_currentToken?.Position.Start ?? "EOF"));
    }

    // ReSharper disable once OutParameterValueIsAlwaysDiscarded.Local
    private bool TryEatToken(TokenKind kind, out TokenInfo token)
    {
        if (_currentToken is { TokenKind: var tokenKind } t && tokenKind == kind)
        {
            _currentToken = _lexer!.NextToken();
            token = t;
            return true;
        }

        token = TokenInfo.Empty;
        return false;
    }

    public Sequence<TContext>? Parse()
    {
        var root = Path();
        return _lexer!.NextToken() is { } token
            ? ThrowUtils.MacroParse<Sequence<TContext>?>(MacroParserResources.UnexpectedTokenFormatted.Format(token.Position.Start.Value - 1))
            : root;
    }

    private Sequence<TContext>? Path()
    {
        return Sequence();
    }

    private Sequence<TContext>? Sequence()
    {
        if (_currentToken is not { TokenKind: TokenKind.RBrace } and not null && SingleNode() is { } node)
            return new Sequence<TContext>(node, Sequence());

        return null;
    }

    private SingleNode<TContext>? SingleNode()
    {
        return _currentToken switch
        {
            { TokenKind: TokenKind.At } => Macro(),
            { TokenKind: TokenKind.PlainText or TokenKind.Equal } => PlainText(),
            { TokenKind: TokenKind.RBrace } => null,
            _ => ThrowUtils.MacroParse<SingleNode<TContext>?>(MacroParserResources.UnexpectedTokenFormatted.Format(_currentToken?.Position.Start is { Value: var start } ? start + 1 : "EOF"))
        };
    }

    private Macro<TContext> Macro()
    {
        _ = EatToken(TokenKind.At);
        _ = EatToken(TokenKind.LBrace);
        var isNot = TryEatToken(TokenKind.Exclamation, out _);
        var macroName = PlainText();
        _expectContextualEqual = true;
        var node = new Macro<TContext>(macroName, OptionalMacroParameter(), isNot);
        _ = EatToken(TokenKind.RBrace);
        return node;
    }

    private OptionalMacroParameter<TContext>? OptionalMacroParameter()
    {
        _expectContextualEqual = false;
        return TryEatToken(TokenKind.Equal, out _) && Sequence() is { } sequence ? new OptionalMacroParameter<TContext>(sequence) : null;
    }

    private PlainText<TContext> PlainText()
    {
        if (_currentToken?.TokenKind is TokenKind.Equal)
        {
            if (_expectContextualEqual)
            {
                return ThrowUtils.MacroParse<PlainText<TContext>>(MacroParserResources.UnexpectedTokenFormatted.Format(_currentToken.Position.Start));
            }

            _ = EatToken(TokenKind.Equal);
            return new PlainText<TContext>($":{EatToken(TokenKind.PlainText).Text}");
        }

        return new PlainText<TContext>(EatToken(TokenKind.PlainText).Text);
    }
}
