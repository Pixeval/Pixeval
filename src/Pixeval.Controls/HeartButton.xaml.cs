using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<XamlUICommand>("Command")]
public sealed partial class HeartButton : UserControl
{
    public HeartButton() => InitializeComponent();

    private void ToggleBookmarkButtonOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        Command.Execute(null);
    }

    public void ScaleIn() => ImageBookmarkButtonGrid.GetResource<Storyboard>("ThumbnailScaleInStoryboard").Begin();

    public void ScaleOut() => ImageBookmarkButtonGrid.GetResource<Storyboard>("ThumbnailScaleOutStoryboard").Begin();
}
