using System;

namespace Pixeval.Controls;

public class TokenAddingEventArgs(Token token, bool cancel) : EventArgs
{
    public Token Token { get; } = token;

    public bool Cancel { get; set; } = cancel;
}
