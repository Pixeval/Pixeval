using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Pixeval.Controls;

public sealed partial class HeartButton : UserControl
{
    [GeneratedDependencyProperty]
    public partial XamlUICommand? Command { get; set; }

    [GeneratedDependencyProperty]
    public partial object? CommandParameter { get; set; }

    public HeartButton() => InitializeComponent();

    [GeneratedDependencyProperty]
    public partial HeartButtonState State { get; set; }

    private double Convert(HeartButtonState value)
    {
        return State switch
        {
            HeartButtonState.Checked => 1,
            HeartButtonState.Unchecked => 0,
            _ => 0.5
        };
    }

    private void HeartButton_OnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (IsTapEnabled)
            Command?.Execute(CommandParameter);
    }
}

public enum HeartButtonState
{
    Unchecked,
    Pending,
    Checked
}
