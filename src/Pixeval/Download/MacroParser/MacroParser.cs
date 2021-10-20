using System;
using Pixeval.Download.MacroParser.Ast;
using Pixeval.Utilities;

namespace Pixeval.Download.MacroParser
{
    public class MacroParser<TContext>
    {
        private Lexer? _lexer;
        private TokenInfo? _currentToken;
        private bool _expectContextualColon;

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

            throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format((object?) _currentToken?.position.Start ?? "EOF"));
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
            return Path();
        }

        private Sequence<TContext>? Path()
        {
            return Sequence();
        }

        private Sequence<TContext>? Sequence()
        {
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
                _ => throw new ArgumentException()
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
                    throw new MacroParseException(MacroParserResources.UnexpectedTokenFormatted.Format(_currentToken.position.Start));
                }

                EatToken(TokenKind.Colon);
                return new PlainText<TContext>($":{EatToken(TokenKind.PlainText).Text}");
            }

            return new PlainText<TContext>(EatToken(TokenKind.PlainText).Text);
        }
    }
}