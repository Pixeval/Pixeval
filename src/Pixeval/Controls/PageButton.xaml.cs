// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Controls;

public sealed partial class PageButton
{
    [GeneratedDependencyProperty]
    public partial string? ToolTip { get; set; }

    [GeneratedDependencyProperty]
    public partial Visibility ButtonVisibility { get; set; }

    [GeneratedDependencyProperty(DefaultValue = true)]
    public partial bool IsPrev { get; set; }

    public PageButton() => InitializeComponent();

    public event RoutedEventHandler? ButtonClick;

    public event RightTappedEventHandler? ButtonRightTapped;

    private void NextButton_OnClicked(object sender, RoutedEventArgs e) => ButtonClick?.Invoke(sender, e);

    private void NextButton_OnRightTapped(object sender, RightTappedRoutedEventArgs e) => ButtonRightTapped?.Invoke(sender, e);

    partial void OnIsPrevPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        if (!IsPrev)
            Image.RenderTransform = new ScaleTransform { ScaleX = -1 };
    }
}
