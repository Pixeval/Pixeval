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
    private readonly Frame _frame;

    private readonly Window _owner;

    /// <summary>
    /// IT IS FORBIDDEN TO USE THIS CONSTRUCTOR DIRECTLY, USE <see cref="WindowFactory.Fork"/> INSTEAD
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="onLoaded"></param>
    internal CustomizableWindow(Window owner, RoutedEventHandler? onLoaded)
    {
        _owner = owner;
        Content = _frame = new Frame()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _frame.Loaded += (_, _) => SetDragRegion();
        _frame.SizeChanged += (_, _) => SetDragRegion();
        Activated += OnActivated;
        Closed += OnClosed;
        _owner.Closed += OnOwnerOnClosed;

        if (onLoaded is not null)
        {
            _frame.Loaded += onLoaded;
        }
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

    private void OnActivated(object sender, WindowActivatedEventArgs e)
    {
        WeakReferenceMessenger.Default.TryRegister<CustomizableWindow, RefreshDragRegionMessage>(this, (_, _) => SetDragRegion());
    }

    private void SetDragRegion()
    {
        if (_frame.Content is ISupportCustomTitleBarDragRegionTest supportCustomTitleBarDragRegion)
        {
            var info = supportCustomTitleBarDragRegion.SetTitleBarDragRegion();
            DragZoneHelper.SetDragZones(info, this);
        }
    }

    public void Navigate<T>(object parameter, NavigationTransitionInfo infoOverride) where T : Page
    {
        _ = _frame.Navigate(typeof(T), parameter, infoOverride);
    }
}
