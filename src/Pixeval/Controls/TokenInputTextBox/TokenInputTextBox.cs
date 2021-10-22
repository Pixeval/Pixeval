#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/TokenInputTextBox.cs
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
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.UserControls.TokenInput;

namespace Pixeval.Controls.TokenInputTextBox
{
    public class TokenInputTextBox : Control
    {
        private const string PartTokenTextBox = "TokenTextBox";
        private const string PartSubmitButton = "SubmitButton";

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(TokenInputTextBox),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty SubmitEnableProperty = DependencyProperty.Register(
            nameof(SubmitEnable),
            typeof(bool),
            typeof(TokenInputTextBox),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        public static readonly DependencyProperty TokenProperty = DependencyProperty.Register(
            nameof(Token),
            typeof(Token),
            typeof(TokenInputTextBox),
            PropertyMetadata.Create(DependencyProperty.UnsetValue));

        private IconButton.IconButton? _submitButton;

        private EventHandler<Token>? _tokenSubmitted;

        private TextBox? _tokenTextBox;

        public TokenInputTextBox()
        {
            DefaultStyleKey = typeof(TokenInputTextBox);
        }

        public string PlaceholderText
        {
            get => (string) GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public bool SubmitEnable
        {
            get => (bool) GetValue(SubmitEnableProperty);
            set => SetValue(SubmitEnableProperty, value);
        }

        public Token Token
        {
            get => (Token) GetValue(TokenProperty);
            set => SetValue(TokenProperty, value);
        }

        public event EventHandler<Token> TokenSubmitted
        {
            add => _tokenSubmitted += value;
            remove => _tokenSubmitted -= value;
        }

        protected override void OnApplyTemplate()
        {
            if (_tokenTextBox is not null)
            {
                _tokenTextBox.KeyDown -= TokenTextBoxOnKeyDown;
            }

            if ((_tokenTextBox = GetTemplateChild(PartTokenTextBox) as TextBox) is not null)
            {
                _tokenTextBox.KeyDown += TokenTextBoxOnKeyDown;
            }

            if (_submitButton is not null)
            {
                _submitButton.Tapped -= SubmitButtonOnTapped;
            }

            if ((_submitButton = GetTemplateChild(PartSubmitButton) as IconButton.IconButton) is not null)
            {
                _submitButton.Tapped += SubmitButtonOnTapped;
            }

            base.OnApplyTemplate();
        }

        private void SubmitButtonOnTapped(object sender, TappedRoutedEventArgs e)
        {
            SubmitToken();
        }

        private void TokenTextBoxOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key is VirtualKey.Enter or VirtualKey.Space)
            {
                SubmitToken();
                e.Handled = true;
            }
        }

        private void SubmitToken()
        {
            if (SubmitEnable && _tokenTextBox is { Text: { Length: > 0 } })
            {
                _tokenSubmitted?.Invoke(this, (Token) Token.Clone());
                _tokenTextBox.Text = string.Empty;
            }
        }
    }
}