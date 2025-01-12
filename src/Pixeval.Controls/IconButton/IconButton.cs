// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls;

[DependencyProperty<string>("Text", propertyChanged: nameof(OnTextChanged))]
[DependencyProperty<IconElement>("Icon", propertyChanged: nameof(OnIconChanged))]
public partial class IconButton : Button
{
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

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = ((StackPanel)((Button)d).Content).FindDescendant<TextBlock>();
        if (e.NewValue is null)
        {
            textBlock!.Visibility = Visibility.Collapsed;
            return;
        }

        textBlock!.Text = (string)e.NewValue;
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((StackPanel)((Button)d).Content).FindDescendant<ContentPresenter>()!.Content = e.NewValue;
    }
}
