// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Pixeval.Util.UI;

public static class ClipService
{
    public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(ClipService), new PropertyMetadata(false, PropertyChangedCallback));

    public static bool GetClipToBounds(DependencyObject obj)
    {
        return (bool)obj.GetValue(ClipToBoundsProperty);
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
        ClipToBounds((FrameworkElement)sender);
    }

    private static void FrameworkElementOnLoaded(object sender, RoutedEventArgs _)
    {
        ClipToBounds((FrameworkElement)sender);
    }

    private static void ClipToBounds(FrameworkElement ele)
    {
        ele.Clip = GetClipToBounds(ele)
            ? new RectangleGeometry { Rect = new Rect(0, 0, ele.ActualWidth, ele.ActualHeight) }
            : null;
    }
}
