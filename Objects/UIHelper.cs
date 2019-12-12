using System;
using System.Windows;
using System.Windows.Media;

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
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            return parentObject switch
            {
                null => null,
                T parent => parent,
                _        => FindVisualParent<T>(parentObject)
            };
        }

        public static T FindChild<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? FindChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static T DataContext<T>(this FrameworkElement element)
        {
            return (T) element.DataContext;
        }

        public static void ScheduleTask(Action action)
        {
            Application.Current.Dispatcher?.BeginInvoke(action);
        }
    }
}