#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MacroParser.cs
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
    private bool _expectContextualColon;
    private Lexer? _lexer;

    public void SetupParsingEnvironment(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
        _expectContextualColon = false;
    }

    private TokenInfo EatToken(TokenKind kind)
    {
        if (_currentToken is { TokenKind: var tokenKind } token && tokenKind == kind)
        {
            _currentToken = _lexer!.NextToken();
            return token;
        }

        throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format((object?) _currentToken?.Position.Start ?? "EOF"));
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

    public IMetaPathNode<TContext>? Parse()
    {
        var root = Path();
        if (_lexer!.NextToken() is { } token)
        {
            throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format(token.Position.Start.Value - 1));
        }

        return root;
    }

    private Sequence<TContext>? Path()
    {
        return Sequence();
    }

    private Sequence<TContext>? Sequence()
    {
        if (_currentToken is { TokenKind: TokenKind.RBrace })
        {
            return null;
        }

        return _currentToken is not null
            ? SingleNode() is { } node
                ? new Sequence<TContext>(node, Sequence())
                : null
            : null;
    }

    private SingleNode<TContext>? SingleNode()
    {
        return _currentToken switch
        {
            { TokenKind: TokenKind.At } => Macro(),
            { TokenKind: TokenKind.PlainText or TokenKind.Colon } => PlainText(),
            { TokenKind: TokenKind.RBrace } => null,
            _ => throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format(_currentToken?.Position.Start is { Value: var start } ? start + 1 : "EOF"))
        };
    }

    private Macro<TContext> Macro()
    {
        EatToken(TokenKind.At);
        EatToken(TokenKind.LBrace);
        var macroName = PlainText();
        _expectContextualColon = true;
        var node = new Macro<TContext>(macroName, OptionalMacroParameter());
        EatToken(TokenKind.RBrace);
        return node;
    }

    private OptionalMacroParameter<TContext>? OptionalMacroParameter()
    {
        _expectContextualColon = false;
        return TryEatToken(TokenKind.Colon, out _) ? new OptionalMacroParameter<TContext>(Sequence()) : null;
    }

    private PlainText<TContext> PlainText()
    {
        if (_currentToken?.TokenKind is TokenKind.Colon)
        {
            if (_expectContextualColon)
            {
                throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format(_currentToken.Position.Start));
            }

            EatToken(TokenKind.Colon);
            return new PlainText<TContext>($":{EatToken(TokenKind.PlainText).Text}");
        }

        return new PlainText<TContext>(EatToken(TokenKind.PlainText).Text);
    }
}