// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using WinUI3Utilities;

namespace Pixeval.Controls;

public sealed partial class HeartButton
{
    [GeneratedDependencyProperty]
    public partial XamlUICommand Command { get; set; }

    [GeneratedDependencyProperty]
    public partial object? CommandParameter { get; set; }

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
