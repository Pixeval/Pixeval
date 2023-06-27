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

using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
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
    internal CustomizableWindow(Window owner)
    {
        _owner = owner;
        Content = _frame = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };
        _frame.Navigated += (_, _) =>
        {
            string info = null;
            //设置为true，这样才能捕获到文件路径名和当前行数，当前行数为GetFrames代码的函数，也可以设置其他参数
            var st = new StackTrace(true);
            //得到当前的所以堆栈
            var sf = st.GetFrames();
            foreach (var t in sf)
            {
                info = info + "\r\n" + " FileName=" + t.GetFileName() + " fullname=" + t.GetMethod().DeclaringType.FullName + " function=" + t.GetMethod().Name + " FileLineNumber=" + t.GetFileLineNumber();
            }
            ;
        };
        Closed += OnClosed;
        _owner.Closed += OnOwnerOnClosed;
    }

    public event RoutedEventHandler FrameLoaded
    {
        add => _frame.Loaded += value;
        remove => _frame.Loaded -= value;
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

    public void SetDragRegion(DragZoneHelper.DragZoneInfo info)
    {
        DragZoneHelper.SetDragZones(info, this);
    }

    public void Navigate<T>(object parameter, NavigationTransitionInfo infoOverride) where T : Page
    {
        _ = _frame.Navigate(typeof(T), parameter, infoOverride);
    }
}
