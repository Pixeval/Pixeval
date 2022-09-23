#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/IconButton.cs
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
using Pixeval.Attributes;


namespace Pixeval.Controls.IconButton;

[DependencyProperty("Text", typeof(string), nameof(OnTextChanged))]
[DependencyProperty("Icon", typeof(IconElement), nameof(OnIconChanged))]
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