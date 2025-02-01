// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.WinUI;

namespace Pixeval.Controls;

public sealed partial class TokenInput
{
    [GeneratedDependencyProperty]
    public partial string? PlaceholderText { get; set; }

    [GeneratedDependencyProperty]
    public partial ICollection<Token> TokenSource { get; set; }

    public TokenInput() => InitializeComponent();

    public Token Token { get; } = new();

    private void TokenInputTextBox_OnTokenSubmitted(object? sender, Token e)
    {
        if (TokenSource.Any(t => t.Equals(Token)))
            return;

        TokenSource.Add(e);
    }
}
