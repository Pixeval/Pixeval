using System;

namespace Pixeval.Download.MacroParser
{
    public record TokenInfo(TokenKind TokenKind, string Text, Range position)
    {
        public static readonly TokenInfo Empty = new(TokenKind.Trivia, string.Empty, Range.All);
    }
}