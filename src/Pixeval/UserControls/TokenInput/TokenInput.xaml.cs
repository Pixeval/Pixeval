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
        public TokenInput()
        {
            InitializeComponent();
            Token = new Token();
        }

        public Token Token { get; }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TokenInput),
            new PropertyMetadata(DependencyProperty.UnsetValue));

        public string PlaceholderText
        {
            get => (string) GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty TokenSourceProperty = DependencyProperty.Register(
            nameof(TokenSource),
            typeof(ICollection<Token>),
            typeof(TokenInput),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public ICollection<Token> TokenSource
        {
            get => (ICollection<Token>) GetValue(TokenSourceProperty);
            set => SetValue(TokenSourceProperty, value);
        }

        private EventHandler<TokenAddingEventArgs>? _tokenAdding;

        public event EventHandler<TokenAddingEventArgs> TokenAdding
        {
            add => _tokenAdding += value;
            remove => _tokenAdding -= value;
        }

        private EventHandler<Token>? _tokenAdded;

        public event EventHandler<Token> TokenAdded
        {
            add => _tokenAdded += value;
            remove => _tokenAdded -= value;
        }

        private EventHandler<TokenDeletingEventArgs>? _tokenDeleting;

        public event EventHandler<TokenDeletingEventArgs> TokenDeleting
        {
            add => _tokenDeleting += value;
            remove => _tokenDeleting -= value;
        }

        private EventHandler<Token>? _tokenDeleted;

        public event EventHandler<Token> TokenDeleted
        {
            add => _tokenDeleted += value;
            remove => _tokenDeleted -= value;
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
            var token = sender.GetDataContext<Token>();
            var arg = new TokenDeletingEventArgs(token, false);
            _tokenDeleting?.Invoke(this, arg);
            if (!arg.Cancel)
            {
                TokenSource.Remove(token);
                _tokenDeleted?.Invoke(this, token);
            }
        }

        public static Visibility CalculateTokenIconRightmostSeparatorVisibility(bool caseSensitive, bool isRegex)
        {
            return (caseSensitive || isRegex).ToVisibility();
        }
    }

    public class TokenAddingEventArgs : EventArgs
    {
        public Token Token { get; }

        public bool Cancel { get; set; }

        public TokenAddingEventArgs(Token token, bool cancel)
        {
            Token = token;
            Cancel = cancel;
        }
    }

    public class TokenDeletingEventArgs : EventArgs
    {
        public Token Token { get; }

        public bool Cancel { get; set; }

        public TokenDeletingEventArgs(Token token, bool cancel)
        {
            Token = token;
            Cancel = cancel;
        }
    }
}