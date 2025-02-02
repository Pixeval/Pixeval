// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls;

public partial class IconButton : Button
{
    [GeneratedDependencyProperty]
    public partial string? Text { get; set; }

    [GeneratedDependencyProperty]
    public partial IconElement? Icon { get; set; }

    public IconButton()
    {
        DefaultStyleKey = typeof(IconButton);
        Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children =
            {
                new ContentPresenter
                {
                    VerticalAlignment = VerticalAlignment.Center
                },
                new TextBlock
                {
                    Margin = new Thickness(5, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    partial void OnTextPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        var textBlock = ((StackPanel)Content).FindDescendant<TextBlock>();
        if (e.NewValue is null)
        {
            textBlock!.Visibility = Visibility.Collapsed;
            return;
        }

        textBlock!.Text = (string)e.NewValue;
    }

    partial void OnIconPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        ((StackPanel)Content).FindDescendant<ContentPresenter>()!.Content = e.NewValue;
    }
}
