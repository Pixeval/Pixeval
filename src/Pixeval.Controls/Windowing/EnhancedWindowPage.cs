// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;

namespace Pixeval.Controls;

public static class EnhancedWindowPageExtension
{
    public static void NavigateTo<T>(this Frame frame, ulong hWnd, object? parameter = null, NavigationTransitionInfo? info = null) where T : EnhancedWindowPage
    {
        _ = frame.Navigate(typeof(T), new NavigateParameter(parameter, hWnd), info);
    }
}

file record NavigateParameter(object? Parameter, ulong HWnd);

public partial class EnhancedWindowPage : EnhancedPage
{
    protected ulong HWnd { get; private set; }

    public sealed override void OnPageActivated(NavigationEventArgs e)
    {
        var parameter = e.Parameter.To<NavigateParameter>();
        HWnd = parameter.HWnd;
        if (this is SupportCustomTitleBarDragRegionPage page)
            Loaded += (_, _) =>
            {
                var window = WindowFactory.ForkedWindows[HWnd];
                page.RaiseSetTitleBarDragRegion(window);
                window.AppWindow.Changed += (s, args) =>
                {
                    // 等待XAML元素变化后计算
                    if (args.DidSizeChange)
                        _ = Task.Delay(500).ContinueWith(_ =>
                             page.RaiseSetTitleBarDragRegion(window), TaskScheduler.FromCurrentSynchronizationContext());
                    else
                        page.RaiseSetTitleBarDragRegion(window);
                };
            };

        OnPageActivated(e, parameter.Parameter);
    }

    protected void Navigate(Type type, Frame frame, object? parameter, NavigationTransitionInfo? info = null)
    {
        _ = frame.Navigate(type, new NavigateParameter(parameter, HWnd), info);
    }

    protected void Navigate<TPage>(Frame frame, object? parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedWindowPage
    {
        Navigate(typeof(TPage), frame, parameter, info);
    }

    protected void NavigateParent<TPage>(object? parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedWindowPage
    {
        Navigate(typeof(TPage), Frame, parameter, info);
    }

    protected void NavigateSelf(object? parameter, NavigationTransitionInfo? info = null)
    {
        Navigate(GetType(), Frame, parameter, info);
    }

    protected void Navigate(Frame frame, NavigationViewTag tag, NavigationTransitionInfo? info = null)
    {
        Navigate(tag.NavigateTo, frame, tag.Parameter, info);
    }

    /// <inheritdoc cref="OnPageActivated(NavigationEventArgs)"/>
    public virtual void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
    }
}
