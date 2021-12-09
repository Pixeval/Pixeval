#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IconButton.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls.IconButton;

public class IconButton : Button
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(IconButton),
        PropertyMetadata.Create(string.Empty, TextPropertyChangedCallback));

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon),
        typeof(IconElement),
        typeof(IconButton),
        PropertyMetadata.Create(DependencyProperty.UnsetValue, IconChangedCallback));

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

    public string Text
    {
        get => (string) GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IconElement Icon
    {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static void TextPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var textBlock = ((StackPanel) ((Button) d).Content).FindDescendant<TextBlock>();
        if (e.NewValue is null)
        {
            textBlock!.Visibility = Visibility.Collapsed;
            return;
        }

        textBlock!.Text = (string) e.NewValue;
    }

    private static void IconChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((StackPanel) ((Button) d).Content).FindDescendant<ContentPresenter>()!.Content = e.NewValue;
    }
}