#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/AppPopup.cs
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

using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Util.UI;
using WinUI3Utilities;

namespace Pixeval.Popups;

public static class PopupManager
{
    private static readonly Dictionary<Guid, AppPopup> OpenPopupsInternal = new();

    public static IReadOnlyDictionary<Guid, AppPopup> OpenPopups => OpenPopupsInternal;

    public static AppPopup CreatePopup(
        IAppPopupContent content,
        double width = double.NaN,
        double height = double.NaN,
        double widthMargin = double.NaN,
        double heightMargin = double.NaN,
        double minWidth = 0,
        double maxWidth = double.MaxValue,
        double minHeight = 0,
        double maxHeight = double.MaxValue,
        bool lightDismiss = false,
        bool useAnimation = true,
        Action<IAppPopupContent>? opening = null,
        Action<IAppPopupContent, object?>? closing = null)
    {
        return new AppPopup(content, width, height, widthMargin, heightMargin, minWidth, maxWidth, minHeight, maxHeight, lightDismiss, useAnimation, opening, closing);
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

    public static void ClosePopup(Guid guid)
    {
        if (OpenPopupsInternal.TryGetValue(guid, out var popup))
        {
            popup.Close();
            OpenPopupsInternal.Remove(guid);
        }
    }
}

public class AppPopup
{
    private readonly Action<IAppPopupContent, object?>? _closing;
    private readonly IAppPopupContent _content;
    private readonly double _height;
    private readonly double _heightMargin;
    private readonly double _maxHeight;
    private readonly double _maxWidth;
    private readonly double _minHeight;
    private readonly double _minWidth;
    private readonly Action<IAppPopupContent>? _opening;
    private readonly bool _useAnimation;
    private readonly double _width;
    private readonly double _widthMargin;

    public AppPopup(
        IAppPopupContent content,
        double width = double.NaN,
        double height = double.NaN,
        double widthMargin = double.NaN,
        double heightMargin = double.NaN,
        double minWidth = 0,
        double maxWidth = double.MaxValue,
        double minHeight = 0,
        double maxHeight = double.MaxValue,
        bool lightDismiss = false,
        bool useAnimation = true,
        Action<IAppPopupContent>? opening = null,
        Action<IAppPopupContent, object?>? closing = null)
    {
        _content = content;
        _width = width;
        _height = height;
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
        UniqueId = content.UniqueId;
        content.UIContent.HorizontalAlignment = HorizontalAlignment.Stretch;
        content.UIContent.VerticalAlignment = VerticalAlignment.Stretch;

        var themeShadow = new ThemeShadow();
        var shadowReceiver = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"],
            Opacity = 0.4
        };
        var popupContentPresenter = new ContentPresenter
        {
            Shadow = themeShadow,
            Translation = new Vector3(0, 0, 40),
            BorderBrush = (Brush)Application.Current.Resources["PixevalBorderBrush"],
            Background = (Brush)Application.Current.Resources["PixevalPanelBackgroundThemeBrush"],
            BorderThickness = new Thickness(0.3),
            CornerRadius = new CornerRadius(10),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Content = content
        };

        themeShadow.Receivers.Add(shadowReceiver);

        if (lightDismiss)
        {
            shadowReceiver.Tapped += OnShadowReceiverOnTapped;
        }
        else
        {
            shadowReceiver.Tapped -= OnShadowReceiverOnTapped;
        }

        void OnShadowReceiverOnTapped(object sender, TappedRoutedEventArgs a)
        {
            PopupManager.ClosePopup(PopupManager.OpenPopups[UniqueId]);
        }

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
            Child = new Grid
            {
                Children =
                {
                    shadowReceiver, popupContentPresenter
                }
            }
        };
        CurrentContext.Window.SizeChanged += WindowOnSizeChanged;
    }

    public Popup Popup { get; }

    public Guid UniqueId { get; }

    private void WindowOnSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        RearrangePopup(args.Size);
    }

    private void RearrangePopup(Size desiredSize)
    {
        var (windowWidth, windowHeight) = (desiredSize.Width, desiredSize.Height);
        var child = (Grid)Popup.Child;
        var container = (ContentPresenter)((Grid)Popup.Child).Children[1];
        Popup.Width = windowWidth;
        Popup.Height = windowHeight;
        child.Width = windowWidth;
        child.Height = windowHeight;
        if (!double.IsNaN(_width))
        {
            container.Width = _width;
        }
        else if (!double.IsNaN(_widthMargin))
        {
            container.Width = Math.Clamp(windowWidth - _widthMargin * 2, _minWidth, _maxWidth);
        }

        if (!double.IsNaN(_height))
        {
            container.Height = _height;
        }
        else if (!double.IsNaN(_heightMargin))
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
        RearrangePopup(App.AppViewModel.GetDpiAwareAppWindowSize());
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
                _closing?.Invoke(_content, _content is ICompletableAppPopupContent completable ? completable.GetCompletionResult() : null);
            };
            storyboard.Begin();
        }
        else
        {
            Popup.IsOpen = false;
            _closing?.Invoke(_content, _content is ICompletableAppPopupContent completable ? completable.GetCompletionResult() : null);
        }
    }

    ~AppPopup()
    {
        CurrentContext.Window.SizeChanged -= WindowOnSizeChanged;
    }
}
