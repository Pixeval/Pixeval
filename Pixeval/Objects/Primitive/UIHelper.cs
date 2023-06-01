﻿#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.

// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.CSharp.RuntimeBinder;

namespace Pixeval.Objects.Primitive
{
    public static class UiHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        /// <summary>
        ///     Little trick, from https://stackoverflow.com/questions/257587/bring-a-window-to-the-front-in-wpf
        /// </summary>
        /// <param name="w">window to active</param>
        public static void GlobalActivate(this Window w)
        {
            const uint NoSize = 0x0001;
            const uint NoMove = 0x0002;
            const uint ShowWindow = 0x0040;
            var interopHelper = new WindowInteropHelper(w);
            var thisWindowThreadId = GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);
            var currentForegroundWindow = GetForegroundWindow();
            var currentForegroundWindowThreadId = GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);
            SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, NoSize | NoMove | ShowWindow);
            AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
            if (w.WindowState == WindowState.Minimized)
            {
                w.WindowState = WindowState.Normal;
            }
            w.Show();
            w.Activate();
        }

        public static void Disable(this FrameworkElement element)
        {
            element.IsEnabled = false;
        }

        public static void Enable(this FrameworkElement element)
        {
            element.IsEnabled = true;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T dependencyObject)
                    {
                        yield return dependencyObject;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T DataContext<T>(this FrameworkElement element)
        {
            return (T) element.DataContext;
        }

        public static async void SetImageSource(object img, ImageSource imgSource)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => ((Image) img).Source = imgSource);
        }

        public static async void ReleaseImage(object img)
        {
            await Application.Current.Dispatcher.InvokeAsync(() => ((Image) img).Source = null);
        }

        public static ObservableCollection<T> NewItemsSource<T>(ItemsControl itemsControl)
        {
            var collection = new ObservableCollection<T>();
            SetItemsSource(itemsControl, collection);
            return collection;
        }

        public static void SetItemsSource(ItemsControl itemsControl, IEnumerable itemSource)
        {
            itemsControl.ItemsSource = itemSource;
        }

        public static void ReleaseItemsSource(ItemsControl listView)
        {
            listView.ItemsSource = null;
        }

        public static void StartDoubleAnimationUseCubicEase(object sender, string path, double from, double to, int milliseconds)
        {
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(milliseconds)) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            Storyboard.SetTarget(doubleAnimation, (DependencyObject) sender);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(path));
            sb.Children.Add(doubleAnimation);
            sb.Begin();
        }

        public static T GetDataContext<T>(this object sender)
        {
            if (sender is FrameworkElement element)
            {
                return element.DataContext<T>();
            }

            throw new NotSupportedException($"parameter must be a derived class of {nameof(FrameworkElement)}");
        }

        public static T GetResources<T>(this FrameworkElement element, string name)
        {
            return (T) element.Resources[name];
        }

        public static void CloseControls(params UIElement[] controls)
        {
            foreach (var control in controls)
            {
                CloseControl(control);
            }
        }

        public static void CloseControl(this UIElement control)
        {
            try
            {
                control.Dispatcher.Invoke(() => ((dynamic) control).IsOpen = false);
            }
            catch (RuntimeBinderException)
            {
                // ignore
            }
        }

        public static void OpenControl(this UIElement control)
        {
            try
            {
                control.Dispatcher.Invoke(() => ((dynamic) control).IsOpen = true);
            }
            catch (RuntimeBinderException)
            {
                // ignore
            }
        }

        public static void Scroll(ScrollViewer sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                sender.LineUp();
                sender.LineUp();
            }
            else
            {
                sender.LineDown();
                sender.LineDown();
            }
        }
    }

    public class PopupHelper
    {
        public static readonly DependencyProperty PopupPlacementTargetProperty = DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopupHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

        public static DependencyObject GetPopupPlacementTarget(DependencyObject obj)
        {
            return (DependencyObject) obj.GetValue(PopupPlacementTargetProperty);
        }

        public static void SetPopupPlacementTarget(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(PopupPlacementTargetProperty, value);
        }

        private static void OnPopupPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var popupPopupPlacementTarget = e.NewValue as DependencyObject;
                var pop = d as Popup;

                var w = Window.GetWindow(popupPopupPlacementTarget ?? throw new InvalidOperationException());
                if (null != w)
                {
                    w.LocationChanged += (sender, args) =>
                    {
                        if (pop != null)
                        {
                            var offset = pop.HorizontalOffset;
                            pop.HorizontalOffset = offset + 1;
                            pop.HorizontalOffset = offset;
                        }
                    };

                    w.SizeChanged += (sender, args) =>
                    {
                        var mi = typeof(Popup).GetMethod("UpdatePosition", BindingFlags.NonPublic | BindingFlags.Instance);
                        try
                        {
                            mi?.Invoke(pop, null);
                        }
                        catch
                        {
                            // ignored
                        }
                    };
                }
            }
        }
    }
}