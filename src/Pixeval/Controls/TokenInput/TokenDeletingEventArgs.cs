using System;

namespace Pixeval.Controls;

public class TokenDeletingEventArgs(Token token, bool cancel) : EventArgs
{
    public Token Token { get; } = token;

    public bool Cancel { get; set; } = cancel;
}
