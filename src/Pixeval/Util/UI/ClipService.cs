#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/ClipService.cs
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

using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Util.UI;

public class ClipService
{
    public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(ClipService), new PropertyMetadata(false, PropertyChangedCallback));

    public static bool GetClipToBounds(DependencyObject obj)
    {
        return (bool) obj.GetValue(ClipToBoundsProperty);
    }

    public static void SetClipToBounds(DependencyObject obj, bool value)
    {
        obj.SetValue(ClipToBoundsProperty, value);
    }

    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d as FrameworkElement is { } ele)
        {
            ClipToBounds(ele);
            ele.Loaded += FrameworkElementOnLoaded;
            ele.SizeChanged += FrameworkElementOnSizeChanged;
        }
    }

    private static void FrameworkElementOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        ClipToBounds((FrameworkElement) sender);
    }

    private static void FrameworkElementOnLoaded(object sender, RoutedEventArgs _)
    {
        ClipToBounds((FrameworkElement) sender);
    }

    private static void ClipToBounds(FrameworkElement ele)
    {
        ele.Clip = GetClipToBounds(ele)
            ? new RectangleGeometry { Rect = new Rect(0, 0, ele.ActualWidth, ele.ActualHeight) }
            : null;
    }
}