using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Pixeval.Objects
{
    public static class UiHelper
    {
        public static T FindChild<T>(this DependencyObject parent, string childName)
            where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (!(child is T))
                {
                    foundChild = FindChild<T>(child, childName);
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
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

        public static T FindChild<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = child as T ?? FindChild<T>(child);
                if (result != null) return result;
            }

            return null;
        }

        public static T DataContext<T>(this FrameworkElement element)
        {
            return (T) element.DataContext;
        }

        public static void SetImageSource(Image img, ImageSource imgSource)
        {
            img.Source = imgSource;
        }

        public static void ReleaseImage(Image img)
        {
            img.Source = null;
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
            var doubleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(milliseconds))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(doubleAnimation, (DependencyObject)sender);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(path));
            sb.Children.Add(doubleAnimation);
            sb.Begin();
        }

        public static T GetDataContext<T>(this object sender)
        {
            if (sender is FrameworkElement element) return element.DataContext<T>();

            throw new NotSupportedException($"parameter must be derived class of {nameof(FrameworkElement)}");
        }

        public static void ScheduleTask(Action action)
        {
            Application.Current.Dispatcher?.BeginInvoke(action);
        }
    }

    public class PopupHelper
    {
        public static readonly DependencyProperty PopupPlacementTargetProperty =
            DependencyProperty.RegisterAttached("PopupPlacementTarget", typeof(DependencyObject), typeof(PopupHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

        public static DependencyObject GetPopupPlacementTarget(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(PopupPlacementTargetProperty);
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