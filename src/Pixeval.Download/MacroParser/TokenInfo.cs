// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;

namespace Pixeval.Download.MacroParser;

public record TokenInfo(TokenKind TokenKind, string Text, Range Position)
{
    public static readonly TokenInfo Empty = new(TokenKind.Trivia, string.Empty, Range.All);
}
