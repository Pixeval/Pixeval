#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/TokenInput.xaml.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Pixeval.Util.UI;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.TokenInput;

[DependencyProperty<string>("PlaceholderText")]
[DependencyProperty<ICollection<Token>>("TokenSource")]
[DependencyProperty<Visibility>("TokenInputTextBoxVisibility", "Microsoft.UI.Xaml.Visibility.Visible")]
[DependencyProperty<bool>("IsTokenTappedDefaultBehaviorEnabled", "true")]
public sealed partial class TokenInput
{
    public TokenInput()
    {
        InitializeComponent();
        Token = new Token();
    }

    public Token Token { get; }

    public event EventHandler<TokenAddingEventArgs>? TokenAdding;

    public event EventHandler<Token>? TokenAdded;

    /// <summary>
    /// Only works when <see cref="IsTokenTappedDefaultBehaviorEnabled" /> is set to <see langword="true" />
    /// </summary>
    public event EventHandler<TokenDeletingEventArgs>? TokenDeleting;

    public event EventHandler<Token>? TokenDeleted;

    public event EventHandler<Token>? TokenTapped;

    private void TokenInputTextBox_OnTokenSubmitted(object? sender, Token e)
    {
        if (TokenSource.Any(t => t.Equals(Token)))
        {
            return;
        }

        var arg = new TokenAddingEventArgs(e, false);
        TokenAdding?.Invoke(this, arg);
        if (!arg.Cancel)
        {
            TokenSource.Add(e);
            TokenAdded?.Invoke(this, e);
        }
    }

    private void TokenContainer_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsTokenTappedDefaultBehaviorEnabled)
        {
            var token = sender.GetDataContext<Token>();
            var arg = new TokenDeletingEventArgs(token, false);
            TokenDeleting?.Invoke(this, arg);
            if (!arg.Cancel)
            {
                _ = TokenSource.Remove(token);
                TokenDeleted?.Invoke(this, token);
            }
        }

        TokenTapped?.Invoke(sender, sender.GetDataContext<Token>());
    }

    public static Visibility CalculateTokenIconRightmostSeparatorVisibility(bool caseSensitive, bool isRegex)
    {
        return (caseSensitive || isRegex).ToVisibility();
    }
}

public class TokenAddingEventArgs(Token token, bool cancel) : EventArgs
{
    public Token Token { get; } = token;

    public bool Cancel { get; set; } = cancel;
}

public class TokenDeletingEventArgs(Token token, bool cancel) : EventArgs
{
    public Token Token { get; } = token;

    public bool Cancel { get; set; } = cancel;
}
