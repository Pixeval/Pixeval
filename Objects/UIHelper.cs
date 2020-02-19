// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.CSharp.RuntimeBinder;

namespace Pixeval.Objects
{
    internal static class UiHelper
    {
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
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    if (child is T dependencyObject) yield return dependencyObject;

                    foreach (var childOfChild in FindVisualChildren<T>(child)) yield return childOfChild;
                }
        }

        public static T DataContext<T>(this FrameworkElement element)
        {
            return (T) element.DataContext;
        }

        public static void SetImageSource(object img, ImageSource imgSource)
        {
            Application.Current.Dispatcher?.Invoke(() => ((Image) img).Source = imgSource);
        }

        public static void ReleaseImage(object img)
        {
            Application.Current.Dispatcher?.Invoke(() => ((Image) img).Source = null);
        }

        public static void HideControl(UIElement element)
        {
            element.Visibility = Visibility.Hidden;
        }

        public static void ShowControl(UIElement element)
        {
            element.Visibility = Visibility.Visible;
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
            var doubleAnimation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(milliseconds))
            {
                EasingFunction = new CubicEase {EasingMode = EasingMode.EaseOut}
            };
            Storyboard.SetTarget(doubleAnimation, (DependencyObject) sender);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(path));
            sb.Children.Add(doubleAnimation);
            sb.Begin();
        }

        public static T GetDataContext<T>(this object sender)
        {
            if (sender is FrameworkElement element) return element.DataContext<T>();

            throw new NotSupportedException($"parameter must be derive class of {nameof(FrameworkElement)}");
        }

        public static T GetResources<T>(this FrameworkElement element, string name)
        {
            return (T) element.Resources[name];
        }

        public static void CloseControls(params UIElement[] controls)
        {
            foreach (var control in controls) CloseControl(control);
        }

        public static void OpenControls(params UIElement[] controls)
        {
            foreach (var control in controls) OpenControl(control);
        }

        public static void CloseControl(this UIElement control)
        {
            try
            {
                ((dynamic) control).IsOpen = false;
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
                ((dynamic) control).IsOpen = true;
            }
            catch (RuntimeBinderException)
            {
                // ignore
            }
        }

        public static void PlayGif(this Image image, IEnumerable<Stream> imageSources, IEnumerable<int> delay, CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                var enumerable = imageSources as Stream[] ?? imageSources.ToArray();
                var delays = delay as int[] ?? delay.ToArray();
                while (!cancellationToken.IsCancellationRequested)
                    for (var i = 0; i < enumerable.Length && !cancellationToken.IsCancellationRequested; i++)
                    {
                        image.Source = PixivEx.FromStream(enumerable[i]);
                        await Task.Delay(delays[i], cancellationToken);
                    }
            }, cancellationToken);
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

    internal class PopupHelper
    {
        public static readonly DependencyProperty PopupPlacementTargetProperty =
            DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopupHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

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
                        var mi = typeof(Popup).GetMethod("UpdatePosition",
                            BindingFlags.NonPublic | BindingFlags.Instance);
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