#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/TokenInput.xaml.cs
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

namespace Pixeval.UserControls.TokenInput
{
    public sealed partial class TokenInput
    {
        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TokenInput),
            new PropertyMetadata(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty TokenSourceProperty = DependencyProperty.Register(
            nameof(TokenSource),
            typeof(ICollection<Token>),
            typeof(TokenInput),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty TokenInputTextBoxVisibilityProperty = DependencyProperty.Register(
            nameof(TokenInputTextBoxVisibility),
            typeof(Visibility),
            typeof(TokenInput),
            PropertyMetadata.Create(Visibility.Visible));

        public static readonly DependencyProperty IsTokenTappedDefaultBehaviorEnabledProperty = DependencyProperty.Register(
            nameof(IsTokenTappedDefaultBehaviorEnabled),
            typeof(bool),
            typeof(TokenInput),
            PropertyMetadata.Create(true));

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

        public string PlaceholderText
        {
            get => (string) GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public ICollection<Token> TokenSource
        {
            get => (ICollection<Token>) GetValue(TokenSourceProperty);
            set => SetValue(TokenSourceProperty, value);
        }

        public Visibility TokenInputTextBoxVisibility
        {
            get => (Visibility) GetValue(TokenInputTextBoxVisibilityProperty);
            set => SetValue(TokenInputTextBoxVisibilityProperty, value);
        }

        public bool IsTokenTappedDefaultBehaviorEnabled
        {
            get => (bool) GetValue(IsTokenTappedDefaultBehaviorEnabledProperty);
            set => SetValue(IsTokenTappedDefaultBehaviorEnabledProperty, value);
        }

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
}