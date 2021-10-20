namespace Pixeval.Download.MacroParser
{
    public class Lexer
    {
        private readonly CharStream _rawString;

        public Lexer(string rawString)
        {
            _rawString = new CharStream(rawString);
        }

        private char _currentChar => _rawString.Peek();

        public TokenInfo? NextToken()
        {
            switch (_currentChar)
            {
                case char.MaxValue:
                    return null;
                case '@':
                    var at = new TokenInfo(TokenKind.At, "@", _rawString.Forward..(_rawString.Forward + 1));
                    _rawString.Advance();
                    return at;
                case '{':
                    var lBrace = new TokenInfo(TokenKind.LBrace, "{", _rawString.Forward..(_rawString.Forward + 1));
                    _rawString.Advance();
                    return lBrace;
                case '}':
                    var rBrace = new TokenInfo(TokenKind.RBrace, "}", _rawString.Forward..(_rawString.Forward + 1));
                    _rawString.Advance();
                    return rBrace;
                case ':':
                    var colon = new TokenInfo(TokenKind.Colon, ":", _rawString.Forward..(_rawString.Forward + 1));
                    _rawString.Advance();
                    return colon;
                default:
                    return PlainText();
            }
        }

        private TokenInfo PlainText()
        {
            var forward = _rawString.Forward;
            _rawString.AdvanceMarker();
            var str = _rawString.GetUntilIf(ch => ch is not '{' and not '}' and not '@' and not ':');
            return new TokenInfo(TokenKind.PlainText, str, forward..(forward + str.Length));
        }
    }
}