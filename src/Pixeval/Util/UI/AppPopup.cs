using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.UserControls;

namespace Pixeval.Util.UI
{
    public static class PopupManager
    {
        private static readonly Dictionary<Guid, AppPopup> OpenPopupsInternal = new();

        public static IReadOnlyDictionary<Guid, AppPopup> OpenPopups => OpenPopupsInternal;

        public static AppPopup CreatePopup(
            FrameworkElement content,
            double widthMargin = double.NaN,
            double heightMargin = double.NaN,
            double minWidth = 0,
            double maxWidth = double.MaxValue,
            double minHeight = 0,
            double maxHeight = double.MaxValue, 
            bool lightDismiss = false,
            bool useAnimation = true,
            Action<FrameworkElement>? opening = null,
            Action<FrameworkElement>? closing = null)
        {
            return new(content, widthMargin, heightMargin, minWidth, maxWidth, minHeight, maxHeight, lightDismiss, useAnimation, opening, closing);
        }

        public static void ShowPopup(AppPopup popup)
        {
            popup.Show();
            OpenPopupsInternal[popup.UniqueId] = popup;
        }

        public static void ClosePopup(AppPopup popup)
        {
            popup.Close();
            OpenPopupsInternal.Remove(popup.UniqueId);
        }
    }

    public class AppPopup
    {
        private readonly double _widthMargin;
        private readonly double _heightMargin;
        private readonly double _minWidth;
        private readonly double _maxWidth;
        private readonly double _minHeight;
        private readonly double _maxHeight;
        private readonly bool _useAnimation;
        private readonly FrameworkElement _content;
        private readonly Action<FrameworkElement>? _opening;
        private readonly Action<FrameworkElement>? _closing;

        public AppPopup(
            FrameworkElement content, 
            double widthMargin = double.NaN,
            double heightMargin = double.NaN,
            double minWidth = 0,
            double maxWidth = double.MaxValue,
            double minHeight = 0,
            double maxHeight = double.MaxValue,
            bool lightDismiss = false,
            bool useAnimation = true,
            Action<FrameworkElement>? opening = null,
            Action<FrameworkElement>? closing = null)
        {
            _content = content;
            _opening = opening;
            _closing = closing;
            _maxWidth = maxWidth;
            _widthMargin = widthMargin;
            _heightMargin = heightMargin;
            _minWidth = minWidth;
            _maxWidth = maxWidth;
            _minHeight = minHeight;
            _maxHeight = maxHeight;
            _useAnimation = useAnimation;
            var (windowWidth, windowHeight) = App.AppViewModel.GetAppWindowSizeTuple();
            UniqueId = Guid.NewGuid();
            content.HorizontalAlignment = HorizontalAlignment.Stretch;
            content.VerticalAlignment = VerticalAlignment.Stretch;
            Popup = new Popup
            {
                RequestedTheme = App.AppViewModel.AppRootFrameTheme,
                Transitions = new TransitionCollection
                {
                    new PopupThemeTransition()
                },
                XamlRoot = App.AppViewModel.AppWindowRootFrame.XamlRoot,
                Width = windowWidth,
                Height = windowHeight,
                Child = new OverlayPopupContent(UniqueId)
                {
                    IsLightDismiss = lightDismiss,
                    Width = windowWidth,
                    Height = windowHeight,
                    PopupContent = new Grid
                    {
                        BorderBrush = (Brush) Application.Current.Resources["PixevalBorderBrush"],
                        Background = (Brush) Application.Current.Resources["PixevalPanelBackgroundThemeBrush"],
                        BorderThickness = new Thickness(0.5),
                        CornerRadius = new CornerRadius(10),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children = {content}
                    },
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                }
            };
            App.AppViewModel.Window.SizeChanged += WindowOnSizeChanged;
        }

        private void WindowOnSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            RearrangePopup(args.Size);
        }

        private void RearrangePopup(Size desiredSize)
        {
            var (windowWidth, windowHeight) = (desiredSize.Width, desiredSize.Height);
            var child = (OverlayPopupContent)Popup.Child;
            var container = (Grid)child.PopupContent;
            Popup.Width = windowWidth;
            Popup.Height = windowHeight;
            child.Width = windowWidth;
            child.Height = windowHeight;
            if (!double.IsNaN(_widthMargin))
            {
                container.Width = Math.Clamp(windowWidth - _widthMargin * 2, _minWidth, _maxWidth);
            }

            if (!double.IsNaN(_heightMargin))
            {
                container.Height = Math.Clamp(windowHeight - _heightMargin * 2, _minHeight, _maxHeight);
            }
        }

        public void Show()
        {
            if (_useAnimation)
            {
                var inAnimation = new PopInThemeAnimation();
                var storyboard = UIHelper.CreateStoryboard(inAnimation);
                Storyboard.SetTarget(inAnimation, Popup);
                storyboard.Begin();
            }
            Popup.IsOpen = true;
            _opening?.Invoke(_content);
            RearrangePopup(App.AppViewModel.GetAppWindowSize());
        }

        public void Close()
        {
            if (_useAnimation)
            {
                var inAnimation = new PopOutThemeAnimation();
                var storyboard = UIHelper.CreateStoryboard(inAnimation);
                Storyboard.SetTarget(inAnimation, Popup);
                storyboard.Completed += (_, _) =>
                {
                    Popup.IsOpen = false;
                    _closing?.Invoke(_content);
                };
                storyboard.Begin();
            }
            else
            {
                Popup.IsOpen = false;
                _closing?.Invoke(_content);
            }
        }

        public Popup Popup { get; }

        public Guid UniqueId { get; }
        
        ~AppPopup()
        {
            App.AppViewModel.Window.SizeChanged -= WindowOnSizeChanged;
        }
    }
}