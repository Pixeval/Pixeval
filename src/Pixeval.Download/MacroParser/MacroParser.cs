// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Pixeval.Download.MacroParser.Ast;

namespace Pixeval.Download.MacroParser;

public class MacroParser<TContext>
{
    private TokenInfo? _currentToken;
    private Lexer? _lexer;

    public void SetupParsingEnvironment(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.NextToken();
    }

    private TokenInfo EatToken(TokenKind kind)
    {
        if (_currentToken is { TokenKind: var tokenKind } token && tokenKind == kind)
        {
            _currentToken = _lexer!.NextToken();
            return token;
        }
        
        throw new MacroParseException(
            MacroParseException.ErrorType.UnexpectedToken,
            _currentToken?.Position.Start.Value.ToString() ?? "EOF",
            _currentToken?.Position.ToTextSpan() ?? new MacroTextSpan(0, 1));
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
            ? throw new MacroParseException(
                MacroParseException.ErrorType.UnexpectedToken,
                (token.Position.Start.Value - 1).ToString(),
                token.Position.ToTextSpan())
            : root;
    }

    private Sequence<TContext>? Path()
    {
        return Sequence(null);
    }

    private Sequence<TContext>? Sequence(TokenKind? stopToken)
    {
        if (_currentToken is not null
            && _currentToken.TokenKind != TokenKind.RBrace
            && _currentToken.TokenKind != stopToken
            && SingleNode(stopToken) is { } node)
        {
            return new(node, Sequence(stopToken));
        }

        return null;
    }

    private SingleNode<TContext>? SingleNode(TokenKind? stopToken)
    {
        return _currentToken switch
        {
            { TokenKind: TokenKind.At } => Macro(),
            { TokenKind: TokenKind.PlainText or TokenKind.Colon } => PlainText(),
            { TokenKind: var tokenKind } when tokenKind == stopToken || tokenKind == TokenKind.RBrace => null,
            _ => throw new MacroParseException(MacroParseException.ErrorType.UnexpectedToken,
                (_currentToken?.Position.Start.Value + 1)?.ToString() ?? "EOF",
                _currentToken?.Position.ToTextSpan() ?? new MacroTextSpan(0, 1))
        };
    }

    private Macro<TContext> Macro()
    {
        _ = EatToken(TokenKind.At);
        _ = EatToken(TokenKind.LBrace);
        var macroName = PlainText();
        var node = new Macro<TContext>(macroName, ConditionalBranches());
        _ = EatToken(TokenKind.RBrace);
        return node;
    }

    private ConditionalMacroBranches<TContext>? ConditionalBranches()
    {
        if (!TryEatToken(TokenKind.Question, out _))
            return null;

        var whenTrue = Sequence(TokenKind.Colon);
        _ = EatToken(TokenKind.Colon);
        var whenFalse = Sequence(TokenKind.RBrace);
        return new ConditionalMacroBranches<TContext>(whenTrue, whenFalse);
    }

    private PlainText<TContext> PlainText()
    {
        if (_currentToken?.TokenKind is TokenKind.Colon)
        {
            var colon = EatToken(TokenKind.Colon);
            return new(":", colon.Position);
        }

        var text = EatToken(TokenKind.PlainText);
        return new(text.Text, text.Position);
    }
}
