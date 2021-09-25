using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Pixeval.Util.UI
{
    public static class PopupHelper
    {
        public static DependencyProperty RootWindowProperty = DependencyProperty.RegisterAttached(
            "RootWindow", 
            typeof(Window), 
            typeof(PopupHelper),
            PropertyMetadata.Create(DependencyProperty.UnsetValue, RootWindowChangedCallback));

        public static DependencyProperty WidthMarginProperty = DependencyProperty.RegisterAttached(
            "WidthMargin",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(double.NaN, WidthMarginChangedCallback));

        public static DependencyProperty HeightMarginProperty = DependencyProperty.RegisterAttached(
            "HeightMargin",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(double.NaN, HeightMarginChangedCallback));

        public static DependencyProperty MinHeightProperty = DependencyProperty.RegisterAttached(
            "MinHeight",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(250d));

        public static DependencyProperty MinWidthProperty = DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(250d));

        public static DependencyProperty MaxHeightProperty = DependencyProperty.RegisterAttached(
            "MinHeight",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(800d));

        public static DependencyProperty MaxWidthProperty = DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(600d));

        public static DependencyProperty HorizontalOffsetBiasProperty = DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(0d));
        
        public static DependencyProperty VerticalOffsetBiasProperty = DependencyProperty.RegisterAttached(
            "MinWidth",
            typeof(double),
            typeof(PopupHelper),
            PropertyMetadata.Create(0d));

        private static void HeightMarginChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (GetRootWindow(d) is { } window)
            {
                RearrangePopup(d, window.Size());
            }
        }

        private static void WidthMarginChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (GetRootWindow(d) is { } window)
            {
                RearrangePopup(d, window.Size());
            }
        }

        private static void RootWindowChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is Window window)
            {
                window.SizeChanged += (_, args) => RearrangePopup(d, args.Size);
                RearrangePopup(d, window.Size());
            }
        }

        public static void RearrangePopup(DependencyObject d, Size windowSize)
        {
            if (d is Popup {Child: FrameworkElement {IsLoaded: true} child} popup)
            {
                var (windowWidth, windowHeight) = (windowSize.Width, windowSize.Height);
                var desireHeightMargin = GetHeightMargin(d);
                var desireWidthMargin = GetWidthMargin(d);
                var calculatedWidth = !double.IsNaN(desireWidthMargin)
                    ? Math.Clamp(windowWidth - desireWidthMargin * 2, GetMinWidth(d), GetMaxWidth(d))
                    : child.ActualWidth;
                var calculatedHeight = !double.IsNaN(desireHeightMargin)
                    ? Math.Clamp(windowHeight - desireHeightMargin * 2, GetMinHeight(d), GetMaxHeight(d))
                    : child.ActualHeight;
                (child.Width, child.Height) = (calculatedWidth, calculatedHeight);
                (popup.HorizontalOffset, popup.VerticalOffset) = ((windowWidth - calculatedWidth) / 2 + GetHorizontalOffsetBias(d), (windowHeight - calculatedHeight) / 2 + GetVerticalOffsetBias(d));
            }
        }

        public static Window GetRootWindow(DependencyObject obj)
        {
            return (Window) obj.GetValue(RootWindowProperty);
        }

        public static void SetRootWindow(DependencyObject obj, Window value)
        {
            obj.SetValue(RootWindowProperty, value);
        }

        public static double GetWidthMargin(DependencyObject obj)
        {
            return (double) obj.GetValue(WidthMarginProperty);
        }

        public static void SetWidthMargin(DependencyObject obj, double value)
        {
            obj.SetValue(WidthMarginProperty, value);
        }

        public static double GetHeightMargin(DependencyObject obj)
        {
            return (double) obj.GetValue(HeightMarginProperty);
        }

        public static void SetHeightMargin(DependencyObject obj, double value)
        {
            obj.SetValue(HeightMarginProperty, value);
        }

        public static double GetMinHeight(DependencyObject obj)
        {
            return (double) obj.GetValue(MinHeightProperty);
        }

        public static void SetMinHeight(DependencyObject obj, double value)
        {
            obj.SetValue(MinHeightProperty, value);
        }

        public static double GetMinWidth(DependencyObject obj)
        {
            return (double) obj.GetValue(MinWidthProperty);
        }

        public static void SetMinWidth(DependencyObject obj, double value)
        {
            obj.SetValue(MinHeightProperty, value);
        }

        public static double GetMaxWidth(DependencyObject obj)
        {
            return (double) obj.GetValue(MaxWidthProperty);
        }

        public static void SetMaxWidth(DependencyObject obj, double value)
        {
            obj.SetValue(MaxWidthProperty, value);
        }

        public static double GetMaxHeight(DependencyObject obj)
        {
            return (double) obj.GetValue(MaxHeightProperty);
        }

        public static void SetMaxHeight(DependencyObject obj, double value)
        {
            obj.SetValue(MaxHeightProperty, value);
        }

        public static double GetHorizontalOffsetBias(DependencyObject obj)
        {
            return (double) obj.GetValue(HorizontalOffsetBiasProperty);
        }

        public static void SetHorizontalOffsetBias(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalOffsetBiasProperty, value);
        }

        public static double GetVerticalOffsetBias(DependencyObject obj)
        {
            return (double) obj.GetValue(VerticalOffsetBiasProperty);
        }

        public static void SetVerticalOffsetBias(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalOffsetBiasProperty, value);
        }
    }
}