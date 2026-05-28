// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace Pixeval.Controls;

[TemplatePart(PartTextBox, typeof(TextBox))]
[PseudoClasses(PseudoClassInvalid)]
public class IPBox : TemplatedControl
{
    private const string PartTextBox = "PART_TextBox";
    private const string PseudoClassInvalid = ":invalid";

    public static readonly StyledProperty<IPAddress?> IPAddressProperty =
        AvaloniaProperty.Register<IPBox, IPAddress?>(nameof(IPAddress), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string?> TextProperty =
        TextBox.TextProperty.AddOwner<IPBox>(new(defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true));

    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        TextBox.PlaceholderTextProperty.AddOwner<IPBox>();

    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        TextBox.PlaceholderForegroundProperty.AddOwner<IPBox>();

    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        TextBox.InnerLeftContentProperty.AddOwner<IPBox>();

    public static readonly StyledProperty<object?> InnerRightContentProperty =
        TextBox.InnerRightContentProperty.AddOwner<IPBox>();

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        TextBox.IsReadOnlyProperty.AddOwner<IPBox>();

    public static readonly StyledProperty<bool> NormalizeOnLostFocusProperty =
        AvaloniaProperty.Register<IPBox, bool>(nameof(NormalizeOnLostFocus), true);

    public static readonly DirectProperty<IPBox, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<IPBox, bool>(nameof(IsValid), box => box.IsValid);

    private bool _updatingIPAddress;
    private bool _updatingText;
    private TextBox? _textBox;

    static IPBox()
    {
        FocusableProperty.OverrideDefaultValue<IPBox>(true);
        TextProperty.Changed.AddClassHandler<IPBox>((box, _) => box.OnTextChanged());
        IPAddressProperty.Changed.AddClassHandler<IPBox>((box, _) => box.OnIPAddressChanged());
    }

    public IPAddress? IPAddress
    {
        get => GetValue(IPAddressProperty);
        set => SetValue(IPAddressProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }

    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }

    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool NormalizeOnLostFocus
    {
        get => GetValue(NormalizeOnLostFocusProperty);
        set => SetValue(NormalizeOnLostFocusProperty, value);
    }

    public bool IsValid
    {
        get;
        private set
        {
            SetAndRaise(IsValidProperty, ref field, value);
            PseudoClasses.Set(PseudoClassInvalid, !value);
        }
    } = true;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_textBox is not null)
            _textBox.LostFocus -= TextBoxOnLostFocus;

        base.OnApplyTemplate(e);

        _textBox = e.NameScope.Find<TextBox>(PartTextBox);
        if (_textBox is not null)
            _textBox.LostFocus += TextBoxOnLostFocus;
    }

    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);

        if (ReferenceEquals(e.Source, this))
            Dispatcher.UIThread.Post(FocusTextBox, DispatcherPriority.Input);
    }

    private void OnTextChanged()
    {
        if (_updatingText)
            return;

        UpdateIPAddressFromText();
    }

    private void OnIPAddressChanged()
    {
        if (_updatingIPAddress)
            return;

        UpdateTextFromIPAddress();
    }

    private void UpdateIPAddressFromText()
    {
        var text = Text?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            IsValid = true;
            SetCurrentIPAddress(null);
            return;
        }

        if (IPAddress.TryParse(text, out var ipAddress))
        {
            IsValid = true;
            SetCurrentIPAddress(ipAddress);
            return;
        }

        IsValid = false;
        SetCurrentIPAddress(null);
    }

    private void UpdateTextFromIPAddress()
    {
        IsValid = true;
        SetCurrentText(IPAddress?.ToString() ?? "");
    }

    private void NormalizeText()
    {
        if (NormalizeOnLostFocus && IsValid && IPAddress is not null)
            SetCurrentText(IPAddress.ToString());
    }

    private void SetCurrentIPAddress(IPAddress? ipAddress)
    {
        if (Equals(IPAddress, ipAddress))
            return;

        _updatingIPAddress = true;
        try
        {
            SetCurrentValue(IPAddressProperty, ipAddress);
        }
        finally
        {
            _updatingIPAddress = false;
        }
    }

    private void SetCurrentText(string text)
    {
        if (string.Equals(Text, text, System.StringComparison.Ordinal))
            return;

        _updatingText = true;
        try
        {
            SetCurrentValue(TextProperty, text);
        }
        finally
        {
            _updatingText = false;
        }
    }

    private void TextBoxOnLostFocus(object? sender, RoutedEventArgs e)
    {
        NormalizeText();
    }

    private void FocusTextBox()
    {
        if (_textBox?.Focus() is true)
            _textBox.CaretIndex = _textBox.Text?.Length ?? 0;
    }
}
