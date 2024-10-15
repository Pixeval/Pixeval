using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("ToolTip")]
[DependencyProperty<Visibility>("ButtonVisibility")]
[DependencyProperty<bool>("IsPrev", "true", propertyChanged: nameof(OnIsPrevChanged))]
public sealed partial class PageButton
{
    public PageButton() => InitializeComponent();

    public event RoutedEventHandler? ButtonClick;

    public event RightTappedEventHandler? ButtonRightTapped;

    private void NextButton_OnClicked(object sender, RoutedEventArgs e) => ButtonClick?.Invoke(sender, e);

    private void NextButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e) => ButtonRightTapped?.Invoke(sender, e);

    private static void OnIsPrevChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
        var button = (PageButton)o;
        if (!button.IsPrev)
            button.Image.RenderTransform = new ScaleTransform { ScaleX = -1 };
    }
}
