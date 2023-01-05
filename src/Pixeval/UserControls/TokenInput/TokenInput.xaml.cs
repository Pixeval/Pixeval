#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/TokenInput.xaml.cs
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
using System.Linq;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using WinUI3Utilities.Attributes;

using Pixeval.Util.UI;

namespace Pixeval.UserControls.TokenInput;

[DependencyProperty<string>("PlaceholderText")]
[DependencyProperty<ICollection<Token>>("TokenSource")]
[DependencyProperty<Visibility>("TokenInputTextBoxVisibility", DefaultValue = "Visibility.Visible")]
[DependencyProperty<bool>("IsTokenTappedDefaultBehaviorEnabled", DefaultValue = "true")]
public sealed partial class TokenInput
{
    private EventHandler<Token>? _tokenAdded;

    private EventHandler<TokenAddingEventArgs>? _tokenAdding;

    private EventHandler<Token>? _tokenDeleted;

    private EventHandler<TokenDeletingEventArgs>? _tokenDeleting;

    private EventHandler<Token>? _tokenTapped;

    public TokenInput()
    {
        InitializeComponent();
        Token = new Token();
    }

    public Token Token { get; }

    public event EventHandler<TokenAddingEventArgs> TokenAdding
    {
        add => _tokenAdding += value;
        remove => _tokenAdding -= value;
    }

    public event EventHandler<Token> TokenAdded
    {
        add => _tokenAdded += value;
        remove => _tokenAdded -= value;
    }

    /// <summary>
    ///     Only works when <see cref="IsTokenTappedDefaultBehaviorEnabled" /> is set to <see langword="true" />
    /// </summary>
    public event EventHandler<TokenDeletingEventArgs> TokenDeleting
    {
        add => _tokenDeleting += value;
        remove => _tokenDeleting -= value;
    }

    public event EventHandler<Token> TokenDeleted
    {
        add => _tokenDeleted += value;
        remove => _tokenDeleted -= value;
    }

    public event EventHandler<Token> TokenTapped
    {
        add => _tokenTapped += value;
        remove => _tokenTapped -= value;
    }

    private void TokenInputTextBox_OnTokenSubmitted(object? sender, Token e)
    {
        if (TokenSource.Any(t => t.Equals(Token)))
        {
            return;
        }

        var arg = new TokenAddingEventArgs(e, false);
        _tokenAdding?.Invoke(this, arg);
        if (!arg.Cancel)
        {
            TokenSource.Add(e);
            _tokenAdded?.Invoke(this, e);
        }
    }

    private void TokenContainer_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsTokenTappedDefaultBehaviorEnabled)
        {
            var token = sender.GetDataContext<Token>();
            var arg = new TokenDeletingEventArgs(token, false);
            _tokenDeleting?.Invoke(this, arg);
            if (!arg.Cancel)
            {
                TokenSource.Remove(token);
                _tokenDeleted?.Invoke(this, token);
            }
        }

        _tokenTapped?.Invoke(sender, sender.GetDataContext<Token>());
    }

    public static Visibility CalculateTokenIconRightmostSeparatorVisibility(bool caseSensitive, bool isRegex)
    {
        return (caseSensitive || isRegex).ToVisibility();
    }
}

public class TokenAddingEventArgs : EventArgs
{
    public TokenAddingEventArgs(Token token, bool cancel)
    {
        Token = token;
        Cancel = cancel;
    }

    public Token Token { get; }

    public bool Cancel { get; set; }
}

public class TokenDeletingEventArgs : EventArgs
{
    public TokenDeletingEventArgs(Token token, bool cancel)
    {
        Token = token;
        Cancel = cancel;
    }

    public Token Token { get; }

    public bool Cancel { get; set; }
}
