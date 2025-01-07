// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Download.MacroParser;

public enum TokenKind
{
    Trivia,
    PlainText,
    At,
    LBrace,
    RBrace,
    Exclamation,
    Equal
}
