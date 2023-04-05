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

using System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Pixeval.Messages;
using WinUI3Utilities;
using WinUI3Utilities.Models;

namespace Pixeval.Util.UI.Windowing;

public sealed class CustomizableWindow<W> : Window where W : IWindowProvider
{
    private readonly W _provider;
    private readonly AppWindow _appWindow;
    private readonly Grid _presenter;

    private CustomizableWindow(W provider)
    {
        _provider = provider;
        var hWnd = Win32Interop.GetWindowIdFromWindow(CurrentContext.HWnd);
        _appWindow = AppWindow.GetFromWindowId(hWnd);
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
        WeakReferenceMessenger.Default.Register<CustomizableWindow<W>, RefreshDragRegionMessage>(this, static (recipient, _) =>
        {
            if (AppWindowTitleBar.IsCustomizationSupported() && recipient._provider.Content is ISupportCustomTitleBarDragRegion iSupportCustomTitleBarDragRegion)
            {
                iSupportCustomTitleBarDragRegion.SetTitleBarDragRegionAsync(recipient._provider.TitleBar, recipient._provider.DragRegions);
            }
        });
    }

    private void ContentOnLoaded(object sender, RoutedEventArgs e)
    {
        switch (_provider.BackdropType)
        {
            case BackdropHelper.BackdropType.Acrylic:
                BackdropHelper.TryApplyAcrylic(this);
                break;
            case BackdropHelper.BackdropType.Mica:
                BackdropHelper.TryApplyMica(this, false);
                break;
            case BackdropHelper.BackdropType.MicaAlt:
                BackdropHelper.TryApplyMica(this);
                break;
            case BackdropHelper.BackdropType.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var content = _provider.Content;
        if (_provider.TitleBar is { } titleBar)
        {
            _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            _appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            _appWindow.TitleBar.ButtonHoverBackgroundColor = CurrentContext.App.Resources["SystemControlBackgroundBaseLowBrush"].To<SolidColorBrush>().Color;
            _appWindow.TitleBar.ButtonForegroundColor = CurrentContext.App.Resources["SystemControlForegroundBaseHighBrush"].To<SolidColorBrush>().Color;

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

public sealed class CustomizableWindow2 : Window
{
    private readonly AppHelper.InitializeInfo _provider;
    private readonly WindowInfo _info;
    private readonly Grid _presenter;
    private readonly FrameworkElement _contentElement;
    private readonly DragZoneHelper.DragZoneInfo _dragZoneInfo;
    private readonly FrameworkElement? _titleBar;

    public CustomizableWindow2(AppHelper.InitializeInfo provider, FrameworkElement content, FrameworkElement? titleBar, DragZoneHelper.DragZoneInfo dragZoneInfo)
    {
        _info = new WindowInfo(this);
        _provider = provider;
        _contentElement = content;
        _dragZoneInfo = dragZoneInfo;
        _titleBar = titleBar;
        AppHelper.Initialize(provider, _info, _titleBar);
        Content = _presenter = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                _titleBar,
                _contentElement
            }
        };
        _contentElement.SizeChanged += OnSizeChanged;
        DragZoneHelper.SetDragZones(_dragZoneInfo, _info);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => DragZoneHelper.SetDragZones(_dragZoneInfo);
}
