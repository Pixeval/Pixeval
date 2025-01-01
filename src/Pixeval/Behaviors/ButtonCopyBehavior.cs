using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using Pixeval.Util.UI;
using WinUI3Utilities.Attributes;

namespace Pixeval.Behaviors;

[DependencyProperty<string>("TargetText")]
public partial class ButtonCopyBehavior : Behavior<Button>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Click += OnClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Click -= OnClick;
    }
    private void OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(TargetText))
            return;
        UiHelper.ClipboardSetText(TargetText);
    }
}
