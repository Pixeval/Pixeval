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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using WinUI3Utilities;

namespace Pixeval.Util.UI.Windowing;

public sealed class CustomizableWindow : Window
{
    private readonly AppHelper.InitializeInfo _provider;

    public Frame Frame { get; }

    private readonly FrameworkElement? _titleBar;

    private readonly Window _owner;

    public static CustomizableWindow Create(
        AppHelper.InitializeInfo provider,
        Window owner,
        FrameworkElement? titleBar = null,
        RoutedEventHandler? onLoaded = null)
    {
        var w = new CustomizableWindow(provider, titleBar, owner);
        if (onLoaded is not null)
        {
            w.Frame.Loaded += onLoaded;
        }

        AppHelper.Initialize(w._provider, w, null, w._titleBar);
        return w;
    }

    private CustomizableWindow(AppHelper.InitializeInfo provider, FrameworkElement? titleBar, Window owner)
    {
        Grid presenter;
        _provider = provider;
        Frame = new Frame();
        _titleBar = titleBar;
        _owner = owner;
        Content = presenter = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Children =
            {
                //  _titleBar,
                Frame
            }
        };
        presenter.SizeChanged += OnSizeChanged;
        Activated += OnActivated;
        Closed += OnClosed;
        _owner.Closed += OnOwnerOnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        _owner.Closed -= OnOwnerOnClosed;
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnOwnerOnClosed(object o, WindowEventArgs windowEventArgs)
    {
        Close();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {

        WeakReferenceMessenger.Default.TryRegister<CustomizableWindow, RefreshDragRegionMessage>(this, async (recipient, _) =>
            {
                if (recipient.Frame.Content is ISupportCustomTitleBarDragRegion supportCustomTitleBarDragRegion)
                {
                    // Here I admit that it is a bit confusing that I call "ISupportCustomTitleBarDragRegion::SetTitleBarDragRegionAsync"
                    // rather than DragZoneHelper::SetDragZones, the reason for this is a API design difference between two authors of
                    // this code, The DragZoneHelper::SetDragZones requires *a known list of draggable zone info in detail*, which we 
                    // cannot obtain at here, en revanche, ISupportCustomTitleBarDragRegion delivers this task to its implementations
                    var rects = await supportCustomTitleBarDragRegion.SetTitleBarDragRegionAsync(null, null);
                    AppWindow.TitleBar.SetDragRectangles(rects);
                }
            });
    }

    private async void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Frame.Content is ISupportCustomTitleBarDragRegion supportCustomTitleBarDragRegion)
        {
            var rects = await supportCustomTitleBarDragRegion.SetTitleBarDragRegionAsync(null, null);
            AppWindow.TitleBar.SetDragRectangles(rects);
        }
    }

    public void Navigate<T>(object parameter, NavigationTransitionInfo infoOverride) where T : Page
    {
        _ = Frame.Navigate(typeof(T), parameter, infoOverride);
    }
}
