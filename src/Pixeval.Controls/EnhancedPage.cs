// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Pixeval.Controls;

public partial class EnhancedPage : Page
{
    public int ActivationCount { get; private set; }

    public bool ClearCacheAfterNavigation { get; set; } = true;

    protected sealed override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ++ActivationCount;
        OnPageActivated(e);
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
    public virtual void OnPageActivated(NavigationEventArgs e)
    {
    }
}
