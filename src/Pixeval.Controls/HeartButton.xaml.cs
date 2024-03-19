using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<XamlUICommand>("Command")]
[DependencyProperty<object>("CommandParameter", IsNullable = true)]
public sealed partial class HeartButton
{
    public HeartButton() => InitializeComponent();

    private void ToggleBookmarkButtonOnTapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true;
        if (IsTapEnabled)
            Command.Execute(CommandParameter);
    }

    // ReSharper disable UnusedMember.Global
    public void ScaleIn() => this.GetResource<Storyboard>("ThumbnailScaleInStoryboard").Begin();

    public void ScaleOut() => this.GetResource<Storyboard>("ThumbnailScaleOutStoryboard").Begin();
    // ReSharper restore UnusedMember.Global
}
