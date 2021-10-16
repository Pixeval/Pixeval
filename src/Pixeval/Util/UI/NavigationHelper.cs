using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Pixeval.Popups;

namespace Pixeval.Util.UI
{
    public static class NavigationHelper
    {
        public static readonly DependencyProperty BackHotKeyProperty = DependencyProperty.RegisterAttached(
            "BackHotKey", 
            typeof(VirtualKey), 
            typeof(NavigationHelper), 
            new PropertyMetadata(DependencyProperty.UnsetValue, BackHotKeyChanged));

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
                        ((IGoBack)d).GoBack();
                    }
                }
            }
        }

        public static readonly DependencyProperty ClosePopupHotKeyProperty = DependencyProperty.RegisterAttached(
            "ClosePopupHotKey", 
            typeof(VirtualKey), 
            typeof(NavigationHelper),
            new PropertyMetadata(DependencyProperty.UnsetValue, ClosePopupHotKey));

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
}