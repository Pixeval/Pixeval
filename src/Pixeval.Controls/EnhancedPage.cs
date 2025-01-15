// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System;
using System.Collections.Generic;
using System.Runtime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Controls.Windowing;

namespace Pixeval.Controls;

public partial class EnhancedPage : Page, IStructuralDisposalCompleter
{
    public List<Action> ChildrenCompletes { get; } = [];

    public bool CompleterRegistered { get; set; }

    public bool CompleterDisposed { get; set; }

    public EnhancedWindow Window => WindowFactory.GetWindowForElement(this);

    public int ActivationCount { get; private set; }

    public bool ClearCacheAfterNavigation { get; set; } = true;

    protected sealed override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        OnPageActivated(e, e.Parameter);
        Loaded += (_, _) =>
        {
            if (this is IStructuralDisposalCompleter completer)
            {

                // Hook the disposal event of current page to its parent
                completer.Hook();
            }
        };
    }

    protected sealed override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    {
        base.OnNavigatingFrom(e);
        OnPageDeactivated(e);

        if (!ClearCacheAfterNavigation)
            return;
        NavigationCacheMode = NavigationCacheMode.Disabled;
        if (Parent is not Frame frame)
            return;
        var cacheSize = frame.CacheSize;
        frame.CacheSize = 0;
        frame.CacheSize = cacheSize;
    }

    /// <inheritdoc cref="OnPageActivated"/>
    /// <remarks>
    /// 只有被导航时才会调用此方法，直接关闭窗口等情况下不会调用此方法
    /// </remarks>
    public virtual void OnPageDeactivated(NavigatingCancelEventArgs e)
    {
    }

    /// <summary>
    /// <see cref="OnNavigatedTo"/>-><br/>
    /// <see cref="OnPageActivated"/>-><br/>
    /// <see cref="FrameworkElement.Loaded"/>-><br/>
    /// <see cref="OnPageDeactivated"/>-><br/>
    /// <see cref="OnNavigatingFrom"/>-><br/>
    /// <see cref="FrameworkElement.Unloaded"/>
    /// </summary>
    public virtual void OnPageActivated(NavigationEventArgs e, object? parameter)
    {
    }

    protected void Navigate(Type type, Frame frame, object? parameter, NavigationTransitionInfo? info = null)
    {
        _ = frame.Navigate(type, parameter, info);
    }

    protected void Navigate<TPage>(Frame frame, object? parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedPage
    {
        Navigate(typeof(TPage), frame, parameter, info);
    }

    protected void NavigateParent<TPage>(object? parameter, NavigationTransitionInfo? info = null) where TPage : EnhancedPage
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

    public virtual void CompleteDisposal()
    {
        Content = null;
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect();
    }
}
