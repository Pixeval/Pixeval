// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Pixeval.Controls;

/// <summary>
/// The <see cref="TokenInputTextBox"/> is intended to be used together with <see cref="TokenInput"/>, it contains a <see cref="TextBox"/>
/// and a <see cref="TokenSubmitted"/> event to help user hook logic when a token is submitted, a token can be marked as case-sensitive
/// or regex
/// </summary>
public partial class TokenInputTextBox : Control
{
    [GeneratedDependencyProperty]
    public partial string? PlaceholderText { get; set; }

    [GeneratedDependencyProperty]
    public partial bool SubmitEnable { get; set; }

    [GeneratedDependencyProperty]
    public partial Token Token { get; set; }

    public event EventHandler<Token>? TokenSubmitted;

    private const string PartTokenTextBox = "TokenTextBox";
    private const string PartSubmitButton = "SubmitButton";

    private Button? _submitButton;

    private TextBox? _tokenTextBox;

    public TokenInputTextBox() => DefaultStyleKey = typeof(TokenInputTextBox);

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
            _submitButton.Click -= SubmitButtonOnClicked;
        }

        if ((_submitButton = GetTemplateChild(PartSubmitButton) as Button) is not null)
        {
            _submitButton.Click += SubmitButtonOnClicked;
        }

        base.OnApplyTemplate();
    }

    private void SubmitButtonOnClicked(object sender, RoutedEventArgs e)
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
        if (SubmitEnable && _tokenTextBox is { Text.Length: > 0 })
        {
            TokenSubmitted?.Invoke(this, Token.DeepClone());
            _tokenTextBox.Text = string.Empty;
        }
    }
}
