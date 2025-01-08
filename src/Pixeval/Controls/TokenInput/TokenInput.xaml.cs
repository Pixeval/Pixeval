// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("PlaceholderText")]
[DependencyProperty<ICollection<Token>>("TokenSource")]
public sealed partial class TokenInput
{
    public TokenInput() => InitializeComponent();

    public Token Token { get; } = new();

    private void TokenInputTextBox_OnTokenSubmitted(object? sender, Token e)
    {
        if (TokenSource.Any(t => t.Equals(Token)))
            return;

        TokenSource.Add(e);
    }
}
