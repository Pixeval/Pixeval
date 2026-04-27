// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser.Ast;

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
        
        throw new MacroParseException(MacroParseException.ErrorType.UnexpectedToken, _currentToken?.Position.Start.Value.ToString() ?? "EOF");
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
            ? throw new MacroParseException(MacroParseException.ErrorType.UnexpectedToken, (token.Position.Start.Value - 1).ToString())
            : root;
    }

    private Sequence<TContext>? Path()
    {
        return Sequence();
    }

    private Sequence<TContext>? Sequence()
    {
        if (_currentToken is not { TokenKind: TokenKind.RBrace } and not null && SingleNode() is { } node)
            return new(node, Sequence());

        return null;
    }

    private SingleNode<TContext>? SingleNode()
    {
        return _currentToken switch
        {
            { TokenKind: TokenKind.At } => Macro(),
            { TokenKind: TokenKind.PlainText or TokenKind.Colon } => PlainText(),
            { TokenKind: TokenKind.RBrace } => null,
            _ => throw new MacroParseException(MacroParseException.ErrorType.UnexpectedToken,
                (_currentToken?.Position.Start.Value + 1)?.ToString() ?? "EOF")
        };
    }

    private Macro<TContext> Macro()
    {
        _ = EatToken(TokenKind.At);
        _ = EatToken(TokenKind.LBrace);
        var isNot = TryEatToken(TokenKind.Exclamation, out _);
        var macroName = PlainText();
        _expectContextualColon = true;
        var node = new Macro<TContext>(macroName, OptionalMacroParameter(), isNot);
        _ = EatToken(TokenKind.RBrace);
        return node;
    }

    private OptionalMacroParameter<TContext>? OptionalMacroParameter()
    {
        _expectContextualColon = false;
        return TryEatToken(TokenKind.Colon, out _) && Sequence() is { } sequence ? new OptionalMacroParameter<TContext>(sequence) : null;
    }

    private PlainText<TContext> PlainText()
    {
        if (_currentToken?.TokenKind is TokenKind.Colon)
        {
            if (_expectContextualColon)
            {
                throw new MacroParseException(MacroParseException.ErrorType.UnexpectedToken, _currentToken.Position.Start.ToString());
            }

            _ = EatToken(TokenKind.Colon);
            return new($":{EatToken(TokenKind.PlainText).Text}");
        }

        return new(EatToken(TokenKind.PlainText).Text);
    }
}
