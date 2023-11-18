#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/IconButton.cs
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
                    Margin = new(5, 0, 0, 0),
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
