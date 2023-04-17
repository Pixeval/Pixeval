#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/CustomizableWindow.cs
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

using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using WinUI3Utilities;

namespace Pixeval.Util.UI.Windowing;

public sealed class CustomizableWindowDylech30th<W> : Window where W : IWindowProvider
{
    private readonly W _provider;
    private readonly Grid _presenter;

    private CustomizableWindowDylech30th(W provider)
    {
        _provider = provider;
        _presenter = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Content = _presenter;
        _presenter.Loaded += ContentOnLoaded;

        Activated += OnActivated;
        Closed += OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        WeakReferenceMessenger.Default.Register<CustomizableWindowDylech30th<W>, RefreshDragRegionMessage>(this, static (recipient, _) =>
        {
            if (AppWindowTitleBar.IsCustomizationSupported() && recipient._provider.Content is ISupportCustomTitleBarDragRegion iSupportCustomTitleBarDragRegion)
            {
                iSupportCustomTitleBarDragRegion.SetTitleBarDragRegionAsync(recipient._provider.TitleBar, recipient._provider.DragRegions);
            }
        });
    }

    private void ContentOnLoaded(object sender, RoutedEventArgs e)
    {
        SystemBackdrop = _provider.BackdropType switch
        {
            BackdropType.None => null,
            BackdropType.Acrylic => new DesktopAcrylicBackdrop(),
            BackdropType.Mica => new MicaBackdrop(),
            BackdropType.MicaAlt => new MicaBackdrop { Kind = MicaKind.BaseAlt },
            BackdropType.Maintain => SystemBackdrop,
            _ => WinUI3Utilities.ThrowHelper.ArgumentOutOfRange<BackdropType, SystemBackdrop>(_provider.BackdropType)
        };

        var content = _provider.Content;
        if (_provider.TitleBar is { } titleBar)
        {
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonHoverBackgroundColor = CurrentContext.App.Resources["SystemControlBackgroundBaseLowBrush"].To<SolidColorBrush>().Color;
            AppWindow.TitleBar.ButtonForegroundColor = CurrentContext.App.Resources["SystemControlForegroundBaseHighBrush"].To<SolidColorBrush>().Color;

            titleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            titleBar.VerticalAlignment = VerticalAlignment.Top;
            titleBar.Height = _provider.TitleBarHeight == 0 ? 48 : _provider.TitleBarHeight;

            content.Margin = content.Margin with { Top = content.Margin.Top + titleBar.Height };

            _presenter.Children.Add(titleBar);
            _presenter.Children.Add(content);
        }
        else
        {
            if (_provider.ExtendsContentIntoTitleBar)
            {
                var titleBarHeight = _provider.TitleBarHeight == 0 ? 48 : _provider.TitleBarHeight;
                content.Margin = content.Margin with { Top = content.Margin.Top + titleBarHeight };
            }
            _presenter.Children.Add(content);
        }
    }

}

public sealed class CustomizableWindow : Window
{
    private readonly AppHelper.InitializeInfo _provider;
    private readonly Grid _presenter;
    public Frame Frame { get; }
    private readonly DragZoneHelper.DragZoneInfo _dragZoneInfo;
    private readonly FrameworkElement? _titleBar;

    public static CustomizableWindow Create(AppHelper.InitializeInfo provider, DragZoneHelper.DragZoneInfo dragZoneInfo, FrameworkElement? titleBar, RoutedEventHandler onloaded)
    {
        var w = new CustomizableWindow(provider, dragZoneInfo, titleBar);
        w.Frame.Loaded += onloaded;
        AppHelper.Initialize(w._provider, w, null, w._titleBar);
        return w;
    }

    private CustomizableWindow(AppHelper.InitializeInfo provider, DragZoneHelper.DragZoneInfo dragZoneInfo, FrameworkElement? titleBar)
    {
        _provider = provider;
        Frame = new Frame();
        _dragZoneInfo = dragZoneInfo;
        _titleBar = titleBar;
        Content = _presenter = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                //  _titleBar,
                Frame
            }
        };
        _presenter.SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => DragZoneHelper.SetDragZones(_dragZoneInfo);

    public void Navigate<T>(object parameter, NavigationTransitionInfo infoOverride) where T : Page
    {
        _ = Frame.Navigate(typeof(T), parameter, infoOverride);
    }
}
