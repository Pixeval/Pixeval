#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/NavigationHelper.cs
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

using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Popups;

namespace Pixeval.Util.UI;

public static class NavigationHelper
{
    public static readonly DependencyProperty BackHotKeyProperty = DependencyProperty.RegisterAttached(
        "BackHotKey",
        typeof(VirtualKey),
        typeof(NavigationHelper),
        new PropertyMetadata(DependencyProperty.UnsetValue, BackHotKeyChanged));

    public static readonly DependencyProperty ClosePopupHotKeyProperty = DependencyProperty.RegisterAttached(
        "ClosePopupHotKey",
        typeof(VirtualKey),
        typeof(NavigationHelper),
        new PropertyMetadata(DependencyProperty.UnsetValue, ClosePopupHotKey));

    public static void SetBackHotKey(DependencyObject element, VirtualKey value)
    {
        element.SetValue(BackHotKeyProperty, value);
    }

    public static VirtualKey GetBackHotKey(DependencyObject element)
    {
        return (VirtualKey) element.GetValue(BackHotKeyProperty);
    }

    private static void BackHotKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Page and IGoBack)
        {
            if (e.NewValue is VirtualKey)
            {
                ((Page) d).KeyDown += OnKeyDown;
            }
            else
            {
                ((Page) d).KeyDown -= OnKeyDown;
            }

            void OnKeyDown(object _, KeyRoutedEventArgs args)
            {
                if (args.Key == (VirtualKey) e.NewValue)
                {
                    ((IGoBack) d).GoBack();
                }
            }
        }
    }

    public static void SetClosePopupHotKey(DependencyObject element, VirtualKey value)
    {
        element.SetValue(ClosePopupHotKeyProperty, value);
    }

    public static VirtualKey GetClosePopupHotKey(DependencyObject element)
    {
        return (VirtualKey) element.GetValue(ClosePopupHotKeyProperty);
    }

    private static void ClosePopupHotKey(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is IAppPopupContent content)
        {
            if (e.NewValue is VirtualKey)
            {
                content.UIContent.KeyDown += UIContentOnKeyDown;
            }
            else
            {
                content.UIContent.KeyDown -= UIContentOnKeyDown;
            }

            void UIContentOnKeyDown(object sender, KeyRoutedEventArgs args)
            {
                if (args.Key == (VirtualKey) e.NewValue)
                {
                    PopupManager.ClosePopup(content.UniqueId);
                }
            }
        }
    }
}