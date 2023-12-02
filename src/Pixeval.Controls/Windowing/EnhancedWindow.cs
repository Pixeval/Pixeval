#region Copyright (c) Pixeval/Pixeval.Controls
// GPL v3 License
// 
// Pixeval/Pixeval.Controls
// Copyright (c) 2023 Pixeval.Controls/EnhancedWindow.cs
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
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Controls.Windowing;

[WindowSizeHelper]
public sealed partial class EnhancedWindow : Window
{
    private readonly Frame _frame;

    private readonly EnhancedWindow? _owner;

    /// <summary>
    /// IT IS FORBIDDEN TO USE THIS CONSTRUCTOR DIRECTLY, USE <see cref="WindowFactory.Fork"/> INSTEAD
    /// </summary>
    internal EnhancedWindow()
    {
        Content = _frame = new Frame
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        Closed += OnClosed;
    }

    /// <summary>
    /// IT IS FORBIDDEN TO USE THIS CONSTRUCTOR DIRECTLY, USE <see cref="WindowFactory.Fork"/> INSTEAD
    /// </summary>
    /// <param name="owner"></param>
    internal EnhancedWindow(EnhancedWindow owner) : this()
    {
        _owner = owner;
        _owner.Closed += OnOwnerOnClosed;
    }

    public event RoutedEventHandler FrameLoaded
    {
        add => _frame.Loaded += value;
        remove => _frame.Loaded -= value;
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void OnOwnerOnClosed(object sender, WindowEventArgs e)
    {
        Close();
    }

    public void SetDragRegion(DragZoneInfo info)
    {
        DragZoneHelper.SetDragZones(info, this);
    }

    public void Navigate<T>(object parameter, NavigationTransitionInfo infoOverride) where T : Page
    {
        _ = _frame.Navigate(typeof(T), parameter, infoOverride);
    }
}
