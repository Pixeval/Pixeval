using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Pixeval.Controls;

public partial class HeartButton : Button
{
    public HeartButton() => InitializeComponent();

    public static readonly StyledProperty<HeartButtonState> StateProperty =
        AvaloniaProperty.Register<WaveProgressHeart, HeartButtonState>(nameof(State));

    public HeartButtonState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public static readonly FuncValueConverter<HeartButtonState, double> Converter = new(value =>
        value switch
        {
            HeartButtonState.Checked => 1,
            HeartButtonState.Unchecked => 0,
            _ => 0.5
        });

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        Back.Value = State is HeartButtonState.Unchecked ? 1 : 0;
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        Back.Value = 0;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e) => e.Handled = true;
}

[Flags]
public enum HeartButtonState
{
    Unchecked = 0,
    Checked = 1,
    Pending = 2,
}
